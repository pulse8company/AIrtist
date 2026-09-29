using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    [Serializable]
    public sealed class AirtistRestorationRecord
    {
        public int bestMask, lastMask;
        public float seconds;
        public int checks;
        public bool measured;
        public int coinsGranted, energyGranted, noHintBonus;
        public int paidStarMask;
        public static int Stars(int mask) => ((mask&1)!=0?1:0)+((mask&2)!=0?1:0)+((mask&4)!=0?1:0);
    }

    public sealed partial class AirtistLandscapePrototypeController
    {
        [SerializeField] private Sprite museumSearchBackground;
        private readonly bool[] replaying=new bool[Chapters.Length];
        private readonly AirtistRestorationRecord[] restorations=new AirtistRestorationRecord[Chapters.Length];
        private readonly AirtistRatingStar[] resultStars=new AirtistRatingStar[3];
        private TMP_Text resultHeading, resultMetrics, resultRewards, resultFact, resultPaintingTitle;
        private TMP_Text resultSubtitle,resultRewardNote,resultCoins,resultEnergy;
        private readonly TMP_Text[] resultCriteria=new TMP_Text[3],resultCriterionValues=new TMP_Text[3];
        private readonly AirtistCompletionCue restorationCue=new AirtistCompletionCue();
        private UnityEngine.UI.Image resultPainting;
        private UnityEngine.UI.Button resultNext,resultCollection,resultReplay;

        private void RecordRestoration()
        {
            var record=restorations[selectedChapter] ?? (restorations[selectedChapter]=new AirtistRestorationRecord());
            var attempt=attempts[selectedChapter];
            var rule=attemptRules!=null?attemptRules.Get(ChapterIds[selectedChapter]):new AirtistAttemptRules.PaintingRule();
            record.measured=attempt!=null && attempt.metricsVersion==1;
            record.seconds=attempt?.elapsedSeconds ?? 0; record.checks=attempt?.checksUsed ?? 0;
            int mask=1;
            if(record.measured && record.seconds<=TimeTarget(rule)) mask|=2;
            if(record.measured && record.checks<=CheckTarget(rule)) mask|=4;
            record.lastMask=mask;
            if(AirtistRestorationRecord.Stars(mask)>AirtistRestorationRecord.Stars(record.bestMask)) record.bestMask=mask;
            record.coinsGranted=record.energyGranted=record.noHintBonus=0;
            chapterCollected[selectedChapter]=true; replaying[selectedChapter]=false;
            restorationCue.Arm(selectedChapter,attempt?.attemptId);
        }
        private float TimeTarget(AirtistAttemptRules.PaintingRule rule) => rule.starSeconds>0?rule.starSeconds:Mathf.Max(1,rule.seconds*.75f);
        private int CheckTarget(AirtistAttemptRules.PaintingRule rule) => rule.starChecks>0?rule.starChecks:Chapters[selectedChapter].Artifacts.Length+1;
        private void ReplayRestoration()
        {
            if(!chapterCollected[selectedChapter] || adPending) return;
            replaying[selectedChapter]=true;
            Array.Clear(artifactFound[selectedChapter],0,artifactFound[selectedChapter].Length);
            hintUsedForChapter[selectedChapter]=false; attempts[selectedChapter]=null;
            areaHintTargets[selectedChapter]=exactHintTargets[selectedChapter]=0;
            ClearTemporaryMarks(); EnsureAttempt(); galleryPanZoom?.ResetView();
            SaveProgress(); Show(Page.Gallery);
        }

        private void BuildRestorationScreen()
        {
            var page=CreatePage(Page.Found,"RestorationResult");
            AirtistFlexibleArtboard.Fill(page);
            page.GetComponent<UnityEngine.UI.Image>().color=new Color(.94f,.90f,.80f);
            var panel=CreatePanel(page,"ResultPaper",new Color(.99f,.96f,.88f),Anchor.Stretch,Vector2.zero,Vector2.zero);
            panel.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            ResultRect(panel,.37f,.04f,.97f,.85f);
            resultPainting=CreateImage(page,"RestoredPainting",GetChapterArtwork(0),Color.white,Anchor.Stretch,Vector2.zero,Vector2.zero,true).GetComponent<UnityEngine.UI.Image>();
            resultPainting.raycastTarget=false;
            ResultRect(resultPainting.rectTransform,.045f,.20f,.335f,.83f);
            AirtistPaintingFrame.Attach(resultPainting);
            resultPaintingTitle=ResultText(page,"PaintingTitle","",25,.045f,.10f,.335f,.19f);
            resultHeading=ResultText(page,"Heading","Картина отреставрирована!",36,.39f,.74f,.95f,.83f);
            resultSubtitle=ResultText(page,"ResultSubtitle","Все AI-дорисовки найдены",22,.40f,.71f,.94f,.74f);
            string[] captions={"Завершение","Время","Точность"};
            for(int i=0;i<3;i++)
            {
                float x=.40f+i*.18f;
                var go=new GameObject("RatingStar"+(i+1),typeof(RectTransform),typeof(CanvasRenderer),typeof(AirtistRatingStar));
                go.transform.SetParent(page,false); ResultRect((RectTransform)go.transform,x,.57f,x+.14f,.71f);
                resultStars[i]=go.GetComponent<AirtistRatingStar>(); resultStars[i].raycastTarget=false;
                resultCriteria[i]=ResultText(page,"Criterion"+i,captions[i],22,x,.52f,x+.14f,.57f);
                resultCriterionValues[i]=ResultText(page,"CriterionValue"+i,"",22,x,.47f,x+.14f,.52f);
            }
            resultMetrics=ResultText(page,"Metrics","",21,.40f,.39f,.94f,.51f);
            resultRewards=ResultText(page,"Rewards","",24,.40f,.29f,.94f,.39f);
            resultCoins=ResultText(page,"CoinsGranted","",26,.45f,.24f,.65f,.30f);
            resultEnergy=ResultText(page,"EnergyGranted","",26,.70f,.24f,.93f,.30f);
            resultRewardNote=ResultText(page,"RewardNote","",18,.40f,.20f,.94f,.24f);
            resultFact=ResultText(page,"PaintingFact","",20,.40f,.17f,.94f,.29f);
            var collection=CreateButton(page,"В коллекцию",new Color(.80f,.87f,.78f),Ink,Anchor.Center,Vector2.zero,Vector2.one,CollectCurrentChapterAndOpenCollection,22);
            resultCollection=collection;
            ResultRect((RectTransform)collection.transform,.40f,.075f,.64f,.15f);
            resultNext=CreateButton(page,"Следующая картина",new Color(.79f,.35f,.25f),Cream,Anchor.Center,Vector2.zero,Vector2.one,CollectCurrentChapterAndOpenNextChapter,22);
            ResultRect((RectTransform)resultNext.transform,.66f,.075f,.94f,.15f);
            var replay=CreateButton(page,"Улучшить результат",new Color(.91f,.87f,.77f),Ink,Anchor.Center,Vector2.zero,Vector2.one,ReplayRestoration,20);
            resultReplay=replay;
            ResultRect((RectTransform)replay.transform,.055f,.035f,.325f,.095f);
            foreach(var button in new[]{collection,resultNext,replay})
            {
                button.image.raycastPadding=Vector4.zero;
                var label=GetButtonLabel(button);
                Stretch(label.rectTransform);
                label.rectTransform.offsetMin=new Vector2(12,4);
                label.rectTransform.offsetMax=new Vector2(-12,-4);
                var scale=label.gameObject.AddComponent<AirtistScaledLabel>();
                scale.artboard=page; scale.designFontSize=22;
            }
        }
        private TMP_Text ResultText(RectTransform root,string name,string text,float font,float x0,float y0,float x1,float y1)
        {
            var label=CreateLabel(root,text,font,Ink,Anchor.Center,Vector2.zero,Vector2.one,TextAlignmentOptions.Center);
            label.name=name; ResultRect(label.rectTransform,x0,y0,x1,y1);
            var scale=label.gameObject.AddComponent<AirtistScaledLabel>(); scale.artboard=root; scale.designFontSize=font;
            return label;
        }
        private static void ResultRect(RectTransform rect,float x0,float y0,float x1,float y1)
        {rect.anchorMin=new Vector2(x0,y0);rect.anchorMax=new Vector2(x1,y1);rect.offsetMin=rect.offsetMax=Vector2.zero;}
        private void RefreshRestorationScreen()
        {
            var record=restorations[selectedChapter];
            int mask=record!=null && record.lastMask!=0?record.lastMask:(IsChapterComplete(selectedChapter)?1:0);
            resultPainting.sprite=GetChapterArtwork(selectedChapter);
            resultPaintingTitle.text=Chapters[selectedChapter].Title;
            for(int i=0;i<3;i++) resultStars[i].color=(mask&(1<<i))!=0?new Color(.94f,.66f,.22f):new Color(.77f,.75f,.68f);
            var rule=attemptRules!=null?attemptRules.Get(ChapterIds[selectedChapter]):new AirtistAttemptRules.PaintingRule();
            bool measured=record!=null && record.measured;
            resultSubtitle.text="Все AI-дорисовки найдены · картина в коллекции";
            resultCriteria[0].text="Все детали";resultCriteria[1].text="Время";resultCriteria[2].text="Точность";
            resultCriterionValues[0].text=FoundArtifactCount(selectedChapter)+" / "+Chapters[selectedChapter].Artifacts.Length;
            resultCriterionValues[1].text=measured?$"{TimeSpan.FromSeconds(record.seconds):mm\\:ss} / {TimeSpan.FromSeconds(TimeTarget(rule)):mm\\:ss}":"Нет оценки";
            resultCriterionValues[2].text=measured?record.checks+" / "+CheckTarget(rule):"Нет оценки";
            for(int i=0;i<3;i++)resultCriterionValues[i].color=(mask&(1<<i))!=0?AirtistApprovedTheme.Teal:AirtistApprovedTheme.Ink;
            resultMetrics.text=measured?"Лучший результат: "+AirtistRestorationRecord.Stars(record.bestMask)+" из 3 · время и проверки: результат / цель":"Пройди заново, чтобы получить оценку времени и точности";
            int grantedCoins=Mathf.Max(0,record?.coinsGranted??0),grantedEnergy=Mathf.Max(0,record?.energyGranted??0);
            resultRewards.text=grantedCoins>0 || grantedEnergy>0?"Награда за это прохождение":"Результат сохранён";
            resultCoins.text="+"+grantedCoins+" монет";resultEnergy.text="+"+grantedEnergy+" энергии";
            resultRewardNote.text=grantedCoins>0 || grantedEnergy>0?"Уже зачислено · можно продолжать расследование"
                :record!=null && (record.paidStarMask&6)==6?"Все награды этой картины уже получены":"Новые звёзды принесут дополнительные монеты";
            resultFact.text=Chapters[selectedChapter].Fact;
            bool hasNext=GetNextUncollectedChapter()>=0;
            resultNext.gameObject.SetActive(hasNext);
            if(AirtistApprovedTheme.Current!=null)
                AirtistApprovedTheme.Rect((RectTransform)resultCollection.transform,hasNext?.482f:.53f,.86f,hasNext?.205f:.39f,.09f);
        }
        private void PresentRestorationCompletion()
        {
            if(!restorationCue.Consume(selectedChapter))return;
            // Consume even when muted: reopening the result must never replay a deferred sound.
            var audio=GetComponent<AirtistMenuAudio>();
            if(audio==null)audio=gameObject.AddComponent<AirtistMenuAudio>();
            audio.PlayPaintingComplete();
        }
    }
}
