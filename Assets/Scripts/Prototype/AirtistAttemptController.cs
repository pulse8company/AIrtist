using System;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        [SerializeField] private AirtistAttemptRules attemptRules;
        [SerializeField] private AirtistContinuationAdProvider continuationAds;
        private int SearchEnergyCost => Mathf.Clamp(Economy.searchEnergy,1,EnergyCap);
        private readonly AirtistAttemptState[] attempts = new AirtistAttemptState[Chapters.Length];
        private readonly bool[] economyRewardGranted = new bool[Chapters.Length];
        private AirtistAttemptHud attemptHud;
        private bool galleryOpen, appPaused, appUnfocused, adPending;
        private double clockStamp, saveStamp;
        private int energy = 100, coins, continuationBonuses;
        private long energyUpdatedUtc;
        public void ConfigureAttemptRules(AirtistAttemptRules rules) => attemptRules = rules;

        private void LoadAttemptProgress(AirtistProgress state)
        {
            energy = state.economyVersion == 0 ? EnergyCap : Mathf.Max(0,state.energy);
            energyAdDay=state.energyAdDay;energyAdsToday=Mathf.Max(0,state.energyAdsToday);RefreshEnergyAdDay();
            coins = Mathf.Max(0,state.coins);
            continuationBonuses = Mathf.Max(0,state.continuationBonuses);
            energyUpdatedUtc = state.energyUpdatedUtc > 0 ? state.energyUpdatedUtc : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            for(int i=0;i<Chapters.Length;i++)
            {
                var entry=Array.Find(state.chapters,c=>c!=null && c.id==ChapterIds[i]);
                attempts[i]=entry?.attempt;
                if (attempts[i] != null && string.IsNullOrEmpty(attempts[i].attemptId)) attempts[i]=null;
                if (attempts[i] != null)
                {
                    attempts[i].clicksLeft=Mathf.Max(0,attempts[i].clicksLeft);
                    if (float.IsNaN(attempts[i].secondsLeft) || float.IsInfinity(attempts[i].secondsLeft)) attempts[i].secondsLeft=0;
                    attempts[i].secondsLeft=Mathf.Max(0,attempts[i].secondsLeft);
                }
                // Existing completed collections are preserved, without retroactive/farmable rewards.
                economyRewardGranted[i]=entry != null && (entry.economyRewardGranted || entry.collected || (state.economyVersion==0 && IsChapterComplete(i)));
                if(state.economyVersion<2 && economyRewardGranted[i] && restorations[i]!=null)restorations[i].paidStarMask=6;
            }
            clockStamp=saveStamp=Time.realtimeSinceStartupAsDouble;
            appUnfocused=!Application.isFocused;
            RefreshEnergy();
        }

        private void SaveAttemptProgress(AirtistProgress state)
        {
            RefreshEnergy();
            state.economyVersion=2; state.energy=energy; state.coins=coins;
            state.energyAdDay=energyAdDay;state.energyAdsToday=energyAdsToday;
            state.energyUpdatedUtc=energyUpdatedUtc; state.continuationBonuses=continuationBonuses;
            for(int i=0;i<Chapters.Length;i++)
            {
                state.chapters[i].attempt=attempts[i];
                state.chapters[i].economyRewardGranted=economyRewardGranted[i];
            }
        }

        private void RefreshEnergy()
        {
            long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if(now<energyUpdatedUtc) return;
            if(energy>=EnergyCap) { energyUpdatedUtc=now; return; }
            long restored=(now-energyUpdatedUtc)/RegenerationSeconds;
            if(restored<=0) return;
            energy=(int)Math.Min(EnergyCap,energy+restored);
            energyUpdatedUtc=energy>=EnergyCap ? now : energyUpdatedUtc+restored*RegenerationSeconds;
        }

        private void EnsureAttempt()
        {
            if(IsChapterComplete(selectedChapter) || attempts[selectedChapter]!=null) return;
            var rule=attemptRules!=null ? attemptRules.Get(ChapterIds[selectedChapter]) : new AirtistAttemptRules.PaintingRule();
            attempts[selectedChapter]=AirtistAttemptState.Start(rule);
            // Migration keeps old finds while charging their checks against the new limit.
            if(attempts[selectedChapter].UsesClicks)
                attempts[selectedChapter].clicksLeft=Mathf.Max(0,attempts[selectedChapter].clicksLeft-FoundArtifactCount(selectedChapter));
        }

        private bool AttemptBlocked => IsChapterComplete(selectedChapter) || (chapterCollected[selectedChapter] && !replaying[selectedChapter])
            || (attempts[selectedChapter]!=null && attempts[selectedChapter].Exhausted);

        private void TickAttemptClock()
        {
            double now=Time.realtimeSinceStartupAsDouble;
            float elapsed=(float)Math.Max(0,now-clockStamp); clockStamp=now;
            if(!galleryOpen || appPaused || appUnfocused || adPending || EnergyShopOpen || AttemptBlocked || energy<SearchEnergyCost) return;
            EnsureAttempt();
            var attempt=attempts[selectedChapter];
            if(attempt==null) return;
            attempt.Tick(elapsed);
            if(attempt.Exhausted) { SaveProgress(); RefreshAttemptHud(); }
        }

        private void Update()
        {
            if(!initialized) return;
            TickAttemptClock(); RefreshEnergy();
            RefreshHeaderWallet();
            RefreshEnergyShop();
            if(galleryOpen) RefreshAttemptHud();
            double now=Time.realtimeSinceStartupAsDouble;
            if(now-saveStamp>=5) { saveStamp=now; SaveProgress(); }
        }

        private void ChangeAttemptPage(Page target)
        {
            TickAttemptClock();
            galleryOpen=target==Page.Gallery;
            if(galleryOpen) EnsureAttempt();
            else ClearTemporaryMarks();
            clockStamp=Time.realtimeSinceStartupAsDouble;
            SaveProgress(); RefreshAttemptHud();
        }

        private bool TrySpendCheck()
        {
            TickAttemptClock(); EnsureAttempt();
            if(!galleryOpen || appPaused || appUnfocused || adPending || EnergyShopOpen || AttemptBlocked) { RefreshAttemptHud(); return false; }
            RefreshEnergy();
            if(energy<SearchEnergyCost) { RefreshAttemptHud(); return false; }
            if(!attempts[selectedChapter].TryCheck()) return false;
            energy-=SearchEnergyCost;
            // Callers save the charge together with the hit/miss result and completion reward.
            return true;
        }

        private void AwardFirstCompletion()
        {
            if(!IsChapterComplete(selectedChapter)) return;
            RecordRestoration();
            RefreshEnergy();
            var record=restorations[selectedChapter];
            bool first=!economyRewardGranted[selectedChapter];
            int newStars=(record.lastMask&6)&~record.paidStarMask;
            int bonus=AirtistRestorationRecord.Stars(newStars)*Mathf.Max(0,Economy.additionalStarCoins);
            int oldCoins=coins, oldEnergy=energy;
            coins=(int)Math.Min(int.MaxValue,(long)coins+(first?Mathf.Max(0,Economy.firstCompletionCoins):0)+bonus);
            if(first && energy<EnergyCap)energy=(int)Math.Min(EnergyCap,(long)energy+Mathf.Max(0,Economy.completionEnergy));
            record.paidStarMask|=newStars;
            record.coinsGranted=coins-oldCoins; record.energyGranted=energy-oldEnergy;
            record.noHintBonus=0;
            if(energy>=EnergyCap) energyUpdatedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            economyRewardGranted[selectedChapter]=true;
        }

        private void RestartAttempt()
        {
            if(adPending || IsChapterComplete(selectedChapter) || (chapterCollected[selectedChapter] && !replaying[selectedChapter])) return;
            Array.Clear(artifactFound[selectedChapter],0,artifactFound[selectedChapter].Length);
            hintUsedForChapter[selectedChapter]=false;
            areaHintTargets[selectedChapter]=exactHintTargets[selectedChapter]=0;
            attempts[selectedChapter]=null; EnsureAttempt(); ClearTemporaryMarks();
            clockStamp=Time.realtimeSinceStartupAsDouble;
            galleryPanZoom?.ResetView();
            SaveProgress(); UpdateProgressLabels(); RefreshAttemptHud();
        }

        private void ContinueWithBonus()
        {
            if(energy<SearchEnergyCost){OpenEnergyShop();return;}
            if(adPending || continuationBonuses<=0 || IsChapterComplete(selectedChapter)) return;
            if(attempts[selectedChapter]?.Continue()!=true) return;
            continuationBonuses--; clockStamp=Time.realtimeSinceStartupAsDouble;
            SaveProgress(); UpdateProgressLabels(); RefreshAttemptHud();
        }

        private void ContinueWithVideo()
        {
            if(energy<SearchEnergyCost){ShowEnergyVideo();return;}
            int chapter=selectedChapter;
            var attempt=attempts[chapter];
            if(adPending || attempt==null || !attempt.Exhausted || attempt.rewardedContinuationUsed
                || continuationAds==null || !continuationAds.IsReady || IsChapterComplete(chapter)) return;
            adPending=true; RefreshAttemptHud();
            bool resolved=false;
            try
            {
                continuationAds.ShowRewarded(earned=>
                {
                    if(resolved) return; resolved=true;
                    if(this==null) return;
                    adPending=false;
                    if(earned && attempts[chapter]==attempt && !attempt.rewardedContinuationUsed && !IsChapterComplete(chapter))
                    {
                        attempt.rewardedContinuationUsed=true;
                        attempt.Continue();
                        // A continuation must leave enough energy for its extra checks.
                        int needed=(int)Math.Min(int.MaxValue,(long)SearchEnergyCost*(attempt.UsesClicks?Math.Max(1,attempt.clicksLeft):3));
                        energy=Math.Max(energy,needed);SaveProgress();
                    }
                    clockStamp=Time.realtimeSinceStartupAsDouble;
                    UpdateProgressLabels(); RefreshAttemptHud();
                });
            }
            catch(Exception ex)
            {
                resolved=true; adPending=false; clockStamp=Time.realtimeSinceStartupAsDouble;
                Debug.LogWarning("AIrtist: rewarded continuation unavailable: "+ex.Message); RefreshAttemptHud();
            }
        }

        private void RefreshAttemptHud()
        {
            RefreshHeaderWallet();
            if(attemptHud==null) return;
            var a=attempts[selectedChapter];
            bool completed=IsChapterComplete(selectedChapter);
            attemptHud.timeLabel.text=(attemptHud.separateCaptions ? "" : "Время  ")+(a!=null && a.UsesTime ? TimeSpan.FromSeconds(Math.Ceiling(a.secondsLeft)).ToString(@"mm\:ss") : "—");
            attemptHud.clicksLabel.text=(attemptHud.separateCaptions ? "" : "Проверки  ")+(a!=null && a.UsesClicks ? a.clicksLeft.ToString() : "—");
            attemptHud.energyLabel.text=(attemptHud.separateCaptions ? "" : "Энергия  ")+energy+"/"+EnergyCap;
            attemptHud.coinsLabel.text=(attemptHud.separateCaptions ? "" : "Монеты  ")+coins;
            bool lowEnergy=!completed && energy<SearchEnergyCost;
            bool limitEnded=!completed && a!=null && a.Exhausted;
            bool failed=limitEnded || lowEnergy;
            attemptHud.endPanel.SetActive(galleryOpen && failed && !EnergyShopOpen);
            if(failed)
            {
                if(lowEnergy)
                {
                    long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    long wait=Math.Max(1,(SearchEnergyCost-energy)*(long)RegenerationSeconds-Math.Max(0,now-energyUpdatedUtc));
                    string remaining=TimeSpan.FromSeconds(wait).ToString(@"mm\:ss");
                    attemptHud.endMessage.text=$"Нужно {SearchEnergyCost} энергии на проверку.\nДо следующего клика: {remaining}.\nНаходки сохранены. Таймер на паузе."
                        +(limitEnded ? "\nЛимит попытки тоже исчерпан." : "");
                }
                else
                {
                    string reason=a.UsesTime && a.secondsLeft<=0 ? "Время закончилось." : "Проверки закончились.";
                    if(a.UsesTime && a.secondsLeft<=0 && a.UsesClicks && a.clicksLeft<=0) reason="Время и проверки закончились.";
                    attemptHud.endMessage.text=reason+"\nПродолжить — сохранить находки.\nЗаново — искать с начала."
                        +(continuationAds==null ? "\nВидео пока недоступно." : "");
                }
            }
            attemptHud.restart.interactable=!adPending && !lowEnergy;
            attemptHud.home.interactable=!adPending;
            if(attemptHud.close!=null) attemptHud.close.interactable=!adPending;
            attemptHud.bonusContinue.interactable=!adPending && (lowEnergy || (limitEnded && continuationBonuses>0));
            GetButtonLabel(attemptHud.bonusContinue).text=lowEnergy?"Пополнить энергию":"Бонусы: "+continuationBonuses;
            attemptHud.rewardedContinue.interactable=lowEnergy?EnergyVideoAvailable:!adPending && limitEnded && a!=null && !a.rewardedContinuationUsed && continuationAds!=null && continuationAds.IsReady;
            GetButtonLabel(attemptHud.rewardedContinue).text=lowEnergy?(EnergyVideoAvailable?$"Видео: +{Economy.rewardedEnergy} энергии":"Видео недоступно"):(a!=null && a.rewardedContinuationUsed ? "Видео использовано" : "Смотреть видео");
            if(galleryHintButton!=null) galleryHintButton.interactable=!adPending;
            RefreshBrushButtons();
        }

        private void OnApplicationFocus(bool focused)
        {
            if(!initialized) return;
            TickAttemptClock(); appUnfocused=!focused; clockStamp=Time.realtimeSinceStartupAsDouble;
            if(!focused) SaveProgress();
        }
        private void OnDisable()
        {
            if(!initialized) return;
            TickAttemptClock(); SaveProgress();
        }
    }
}
