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
        private UnityEngine.UI.Image resultPainting;
        private UnityEngine.UI.Button resultNext;

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
            ResultRect(panel,.37f,.04f,.97f,.85f);
            resultPainting=CreateImage(page,"RestoredPainting",GetChapterArtwork(0),Color.white,Anchor.Stretch,Vector2.zero,Vector2.zero,true).GetComponent<UnityEngine.UI.Image>();
            ResultRect(resultPainting.rectTransform,.045f,.20f,.335f,.83f);
            AirtistPaintingFrame.Attach(resultPainting);
            resultPaintingTitle=ResultText(page,"PaintingTitle","",25,.045f,.10f,.335f,.19f);
            resultHeading=ResultText(page,"Heading","Картина отреставрирована!",36,.39f,.74f,.95f,.83f);
            string[] captions={"Завершение","Время","Точность"};
            for(int i=0;i<3;i++)
            {
                float x=.40f+i*.18f;
                var go=new GameObject("RatingStar"+(i+1),typeof(RectTransform),typeof(CanvasRenderer),typeof(AirtistRatingStar));
                go.transform.SetParent(page,false); ResultRect((RectTransform)go.transform,x,.57f,x+.14f,.71f);
                resultStars[i]=go.GetComponent<AirtistRatingStar>(); resultStars[i].raycastTarget=false;
                ResultText(page,"Criterion"+i,captions[i],22,x,.52f,x+.14f,.57f);
            }
            resultMetrics=ResultText(page,"Metrics","",21,.40f,.39f,.94f,.51f);
            resultRewards=ResultText(page,"Rewards","",24,.40f,.29f,.94f,.39f);
            resultFact=ResultText(page,"PaintingFact","",20,.40f,.17f,.94f,.29f);
            var collection=CreateButton(page,"В коллекцию",new Color(.80f,.87f,.78f),Ink,Anchor.Center,Vector2.zero,Vector2.one,CollectCurrentChapterAndOpenCollection,22);
            ResultRect((RectTransform)collection.transform,.40f,.075f,.64f,.15f);
            resultNext=CreateButton(page,"Следующая картина",new Color(.79f,.35f,.25f),Cream,Anchor.Center,Vector2.zero,Vector2.one,CollectCurrentChapterAndOpenNextChapter,22);
            ResultRect((RectTransform)resultNext.transform,.66f,.075f,.94f,.15f);
            var replay=CreateButton(page,"Улучшить результат",new Color(.91f,.87f,.77f),Ink,Anchor.Center,Vector2.zero,Vector2.one,ReplayRestoration,20);
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
            resultMetrics.text=record!=null && record.measured
                ? $"Время: {TimeSpan.FromSeconds(record.seconds):mm\\:ss} / {TimeSpan.FromSeconds(TimeTarget(rule)):mm\\:ss}\nПроверки: {record.checks} / {CheckTarget(rule)} · Лучший результат: {AirtistRestorationRecord.Stars(record.bestMask)} из 3"
                : "Картина сохранена. Пройди заново, чтобы получить оценку времени и точности.";
            resultRewards.text=record!=null && record.coinsGranted>0
                ? $"Получено: {record.coinsGranted} монет · {record.energyGranted} энергии"+(record.noHintBonus>0?"\nВключая +10 монет без подсказок":"")
                : "Картина в коллекции · повторная награда не выдаётся";
            resultFact.text=Chapters[selectedChapter].Fact;
            resultNext.gameObject.SetActive(GetNextUncollectedChapter()>=0);
        }
    }
}
