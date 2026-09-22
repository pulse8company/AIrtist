using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        [SerializeField] private AirtistEconomyBalance economyBalance;
        private AirtistEconomyBalance fallbackEconomy;
        private AirtistEconomyBalance Economy
        {
            get
            {
                if(economyBalance==null) economyBalance=Resources.Load<AirtistEconomyBalance>("EconomyBalance");
                if(economyBalance!=null) return economyBalance;
                if(fallbackEconomy==null){fallbackEconomy=ScriptableObject.CreateInstance<AirtistEconomyBalance>();fallbackEconomy.hideFlags=HideFlags.HideAndDontSave;}
                return fallbackEconomy;
            }
        }
        private int EnergyCap=>Mathf.Max(1,Economy.energyCap);
        private int RegenerationSeconds=>Mathf.Max(1,Economy.regenerationSeconds);
        private long energyAdDay;
        private int energyAdsToday;
        private RectTransform energyShop;
        private TMP_Text energyShopInfo, energyShopStatus;
        private UnityEngine.UI.Button energyBuy, energyVideo, energyClose, energyStore;
        private bool EnergyShopOpen=>energyShop!=null && energyShop.gameObject.activeSelf;
        private string energyShopMessage="";

        private void RefreshEnergyAdDay()
        {
            long today=DateTimeOffset.UtcNow.ToUnixTimeSeconds()/86400;
            // Moving the local clock backwards must not replenish the quota.
            if(today>energyAdDay){energyAdDay=today;energyAdsToday=0;}
        }
        private void BuildEnergyShop()
        {
            energyShop=CreatePanel((RectTransform)transform,"EnergyShop",new Color(0,0,0,.55f),Anchor.Stretch,Vector2.zero,Vector2.zero);
            Stretch(energyShop);
            var paper=CreatePanel(energyShop,"Paper",AirtistApprovedTheme.Paper,Anchor.Center,Vector2.zero,Vector2.one);
            AirtistApprovedTheme.Rect(paper,.22f,.17f,.56f,.68f);
            var theme=AirtistApprovedTheme.Current;theme?.Surface(paper.GetComponent<UnityEngine.UI.Image>(),AirtistApprovedTheme.Paper);
            Label("Title","Энергия для открытий",.08f,.055f,.80f,.12f,34);
            energyShopInfo=Label("Balance","",.08f,.205f,.84f,.16f,25);
            energyShopStatus=Label("Status","",.08f,.39f,.84f,.15f,21);
            energyBuy=Action("Buy","",.055f,.58f,.43f,.16f,AirtistApprovedTheme.Honey,BuyEnergy);
            energyVideo=Action("Video","",.515f,.58f,.43f,.16f,AirtistApprovedTheme.Lavender,ShowEnergyVideo);
            energyClose=Action("Wait","Подождать / закрыть",.055f,.795f,.43f,.13f,AirtistApprovedTheme.Paper,CloseEnergyShop);
            energyStore=Action("Store","Магазин монет",.515f,.795f,.43f,.13f,AirtistApprovedTheme.Sage,()=>{CloseEnergyShop();Show(Page.Store);});
            Action("Close","×",.915f,.025f,.065f,.095f,AirtistApprovedTheme.Paper,CloseEnergyShop);
            energyShop.gameObject.SetActive(false);
            if(headerEnergy!=null)
            {
                var hit=CreateButton(universalHeader,"",Color.clear,Color.clear,Anchor.Center,Vector2.zero,Vector2.one,OpenEnergyShop,1);
                hit.name="EnergyRefill";AirtistApprovedTheme.Rect((RectTransform)hit.transform,.15f,.008f,.125f,.064f);
                hit.image.raycastPadding=Vector4.zero;
                var plus=CreateLabel((RectTransform)hit.transform,"+",22,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,TextAlignmentOptions.Center);
                AirtistApprovedTheme.Rect(plus.rectTransform,.85f,.15f,.15f,.7f);theme?.Typography(plus,(RectTransform)transform,22);
            }
            TMP_Text Label(string name,string text,float x,float y,float w,float h,float size)
            {
                var label=CreateLabel(paper,text,size,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,TextAlignmentOptions.Center);label.name=name;
                AirtistApprovedTheme.Rect(label.rectTransform,x,y,w,h);theme?.Typography(label,(RectTransform)transform,size);return label;
            }
            UnityEngine.UI.Button Action(string name,string text,float x,float y,float w,float h,Color color,System.Action action)
            {
                var b=CreateButton(paper,text,color,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,action,24);b.name=name;
                AirtistApprovedTheme.Rect((RectTransform)b.transform,x,y,w,h);theme?.Button(b,color);
                var label=GetButtonLabel(b);AirtistApprovedTheme.Rect(label.rectTransform,.04f,.05f,.92f,.9f);theme?.Typography(label,(RectTransform)transform,24);return b;
            }
        }
        private void OpenEnergyShop()
        {
            if(adPending || energyShop==null) return;
            TickAttemptClock();RefreshEnergy();energyShopMessage="";
            energyShop.gameObject.SetActive(true);energyShop.SetAsLastSibling();RefreshEnergyShop();
        }
        private void CloseEnergyShop()
        {
            if(adPending || energyShop==null) return;
            energyShop.gameObject.SetActive(false);clockStamp=Time.realtimeSinceStartupAsDouble;
            SaveProgress();RefreshAttemptHud();
        }
        private void BuyEnergy()
        {
            if(adPending || !EnergyShopOpen) return;
            RefreshEnergy();int price=Mathf.Max(1,Economy.energyPackCoins),amount=Mathf.Max(1,Economy.energyPackAmount);
            if(coins<price || energy>int.MaxValue-amount){RefreshEnergyShop();return;}
            coins-=price;energy+=amount;
            if(energy>=EnergyCap)energyUpdatedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // Both currencies are saved in the same progress record; rapid double taps cannot repurchase.
            SaveProgress();CloseEnergyShop();UpdateProgressLabels();
        }
        private bool EnergyVideoAvailable
        {
            get
            {
                RefreshEnergyAdDay();var a=attempts[selectedChapter];
                return !adPending && energyAdsToday<Mathf.Max(0,Economy.rewardedEnergyDailyLimit)
                    && energy<=int.MaxValue-Mathf.Max(1,Economy.rewardedEnergy)
                    && !(galleryOpen && a!=null && a.Exhausted && a.rewardedContinuationUsed)
                    && continuationAds!=null && continuationAds.IsReady;
            }
        }
        private void ShowEnergyVideo()
        {
            if(!EnergyVideoAvailable) return;
            TickAttemptClock();var attempt=galleryOpen?attempts[selectedChapter]:null;
            int chapter=selectedChapter,amount=Mathf.Max(1,Economy.rewardedEnergy);
            adPending=true;RefreshEnergyShop();RefreshAttemptHud();bool resolved=false;
            try
            {
                continuationAds.ShowRewarded(earned=>
                {
                    if(resolved)return;resolved=true;if(this==null)return;adPending=false;
                    if(earned)
                    {
                        RefreshEnergy();RefreshEnergyAdDay();energyAdsToday++;
                        energy=(int)Math.Min(int.MaxValue,(long)energy+amount);
                        if(energy>=EnergyCap)energyUpdatedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        if(attempt!=null && attempts[chapter]==attempt && attempt.Exhausted && !attempt.rewardedContinuationUsed && !IsChapterComplete(chapter))
                        {attempt.rewardedContinuationUsed=true;attempt.Continue();}
                        SaveProgress();energyShopMessage=$"Получено {amount} энергии.";
                    }
                    else energyShopMessage="Награда не получена. Можно попробовать позже.";
                    clockStamp=Time.realtimeSinceStartupAsDouble;
                    if(earned && EnergyShopOpen)CloseEnergyShop();
                    RefreshEnergyShop();UpdateProgressLabels();RefreshAttemptHud();
                });
            }
            catch(Exception ex)
            {
                if(resolved)return;resolved=true;adPending=false;clockStamp=Time.realtimeSinceStartupAsDouble;
                energyShopMessage="Видео недоступно. Ничего не списано.";
                Debug.LogWarning("AIrtist energy reward: "+ex.Message);RefreshEnergyShop();RefreshAttemptHud();
            }
        }
        private void RefreshEnergyShop()
        {
            if(!EnergyShopOpen)return;
            RefreshEnergyAdDay();int price=Mathf.Max(1,Economy.energyPackCoins),amount=Mathf.Max(1,Economy.energyPackAmount);
            long wait=Math.Max(1,RegenerationSeconds-Math.Max(0,DateTimeOffset.UtcNow.ToUnixTimeSeconds()-energyUpdatedUtc));
            string recovery=energy>=EnergyCap?"Бесплатный запас полон.":$"+1 через {TimeSpan.FromSeconds(wait):mm\\:ss}";
            energyShopInfo.text=$"Энергия: {energy}/{EnergyCap} · Монеты: {coins}\n{recovery}";
            var a=attempts[selectedChapter];bool exhausted=galleryOpen && a!=null && a.Exhausted;
            energyShopStatus.text=energyShopMessage!=""?energyShopMessage:exhausted
                ? "Лимит попытки тоже исчерпан. Покупка даёт только энергию. Видео также продолжит попытку, если ещё не использовано."
                : "Находки сохранены. Таймер на паузе.\nКупленная энергия сохраняется сверх лимита.";
            GetButtonLabel(energyBuy).text=$"+{amount} энергии\n{price} монет";
            energyBuy.interactable=!adPending && coins>=price && energy<=int.MaxValue-amount;
            int left=Math.Max(0,Economy.rewardedEnergyDailyLimit-energyAdsToday);
            GetButtonLabel(energyVideo).text=adPending?"Ожидаем результат…":left==0?"Лимит видео на сегодня":EnergyVideoAvailable?$"Видео: +{Economy.rewardedEnergy}\nОсталось сегодня: {left}":"Видео недоступно";
            energyVideo.interactable=EnergyVideoAvailable;
            energyClose.interactable=energyStore.interactable=!adPending;
        }
    }
}
