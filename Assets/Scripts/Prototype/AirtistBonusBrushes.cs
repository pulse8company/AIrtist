using System;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        [SerializeField,Min(1)] private int areaBrushCost=1, exactBrushCost=2, removeBrushCost=3;
        private readonly int[] areaHintTargets=new int[Chapters.Length], exactHintTargets=new int[Chapters.Length];
        private RectTransform areaHintVisual;
        private bool ToolsAvailable => galleryOpen && !appPaused && !appUnfocused && !adPending && !EnergyShopOpen && !AttemptBlocked;
        private bool ActiveHint(int encoded) => encoded>0 && encoded<=artifactFound[selectedChapter].Length && !artifactFound[selectedChapter][encoded-1];
        private int BrushTarget(bool excludeExact=false)
        {
            for(int i=0;i<artifactFound[selectedChapter].Length;i++)
                if(!artifactFound[selectedChapter][i] && (!excludeExact || exactHintTargets[selectedChapter]!=i+1)) return i;
            return -1;
        }
        private Vector2 HintPoint(int index)
        {
            var entry=hiddenObjects!=null?hiddenObjects.Find(ChapterIds[selectedChapter],index):null;
            return entry!=null?entry.position:Chapters[selectedChapter].Artifacts[index].NormalizedPosition;
        }
        private void UseBonusBrush(int kind)
        {
            TickAttemptClock(); EnsureAttempt();
            if(!ToolsAvailable) return;
            if(kind==3) {ShowBrushVideo();return;}
            if(kind<0 || kind>2) return;
            if(kind==0 && (ActiveHint(areaHintTargets[selectedChapter]) || ActiveHint(exactHintTargets[selectedChapter]))) return;
            if(kind==1 && ActiveHint(exactHintTargets[selectedChapter])) return;
            int target=BrushTarget(kind==2);
            if(target<0) return;
            int cost=Mathf.Max(1,kind==0?areaBrushCost:kind==1?exactBrushCost:removeBrushCost);
            if(hintCount<cost) {galleryFeedback.text=$"Нужно подсказок: {cost}. В запасе: {hintCount}.";return;}
            hintCount-=cost; hintUsedForChapter[selectedChapter]=true;
            if(kind==2) {CompleteAssistedObject(target,false);return;}
            if(kind==0) areaHintTargets[selectedChapter]=target+1;
            else exactHintTargets[selectedChapter]=target+1;
            SelectWorkingTool(false);
            var point=HintPoint(target);
            if(kind==0) point=new Vector2((Mathf.Clamp(Mathf.Floor(point.x*3),0,2)+.5f)/3,(Mathf.Clamp(Mathf.Floor(point.y*3),0,2)+.5f)/3);
            galleryPanZoom?.FocusOn(point);
            SaveProgress(); UpdateProgressLabels(); RefreshBrushButtons();
            galleryFeedback.text=kind==0?"Ищи внутри выделенной области.":"Предмет выделен. Один тап по нему — без энергии и расхода проверок.";
        }
        private void CompleteAssistedObject(int index,bool tapped)
        {
            if(!ToolsAvailable || index<0 || index>=artifactFound[selectedChapter].Length || artifactFound[selectedChapter][index]) return;
            // Bonus finding is not charged as a paid check. A physical confirmation still counts for accuracy.
            if(tapped && attempts[selectedChapter]!=null) attempts[selectedChapter].checksUsed++;
            artifactFound[selectedChapter][index]=true;
            hintUsedForChapter[selectedChapter]=true;
            if(exactHintTargets[selectedChapter]==index+1) exactHintTargets[selectedChapter]=0;
            if(areaHintTargets[selectedChapter]==index+1) areaHintTargets[selectedChapter]=0;
            AwardFirstCompletion(); SaveProgress(); UpdateProgressLabels(); RefreshAttemptHud();
            if(IsChapterComplete(selectedChapter)) OpenFoundForCurrentChapter();
            else galleryFeedback.text=tapped?"Предмет найден бонусной кистью.":"Один предмет убран — его больше не нужно искать.";
        }
        private void ShowBrushVideo()
        {
            if(!ToolsAvailable || ActiveHint(exactHintTargets[selectedChapter])) return;
            if(continuationAds==null || !continuationAds.IsReady) {galleryFeedback.text="Рекламное видео пока недоступно.";return;}
            int chapter=selectedChapter;
            var attempt=attempts[chapter];
            adPending=true; RefreshAttemptHud();
            bool resolved=false;
            try
            {
                continuationAds.ShowRewarded(earned=>
                {
                    if(resolved) return; resolved=true;
                    if(this==null) return;
                    adPending=false;
                    if(earned && attempts[chapter]==attempt && !IsChapterComplete(chapter))
                    {
                        int target=-1;
                        for(int i=0;i<artifactFound[chapter].Length;i++) if(!artifactFound[chapter][i]) {target=i;break;}
                        if(target>=0)
                        {
                            exactHintTargets[chapter]=target+1;hintUsedForChapter[chapter]=true;
                            SaveProgress();
                            if(chapter==selectedChapter) {SelectWorkingTool(false);galleryPanZoom?.FocusOn(HintPoint(target));}
                        }
                    }
                    clockStamp=Time.realtimeSinceStartupAsDouble;
                    UpdateProgressLabels(); RefreshAttemptHud();
                    if(galleryFeedback!=null) galleryFeedback.text=earned?"Подсказка получена: тапни по выделенному предмету.":"Просмотр не завершён. Подсказка не списана.";
                });
            }
            catch(Exception e)
            {
                resolved=true;adPending=false;clockStamp=Time.realtimeSinceStartupAsDouble;
                Debug.LogWarning("AIrtist: brush video unavailable: "+e.Message);
                galleryFeedback.text="Видео недоступно. Подсказки не списаны.";RefreshAttemptHud();
            }
        }
        private void RefreshBrushButtons()
        {
            if(gameplayScreen==null || gameplayScreen.brushes==null) return;
            bool available=ToolsAvailable, exact=ActiveHint(exactHintTargets[selectedChapter]);
            int[] costs={Mathf.Max(1,areaBrushCost),Mathf.Max(1,exactBrushCost),Mathf.Max(1,removeBrushCost)};
            for(int i=0;i<3;i++)
            {
                bool usable=i==0?!exact && !ActiveHint(areaHintTargets[selectedChapter]):i==1?!exact:BrushTarget(true)>=0;
                gameplayScreen.brushes[i].interactable=available && usable && hintCount>=costs[i];
                if(gameplayScreen.brushCosts!=null && i<gameplayScreen.brushCosts.Length && gameplayScreen.brushCosts[i]!=null)
                    gameplayScreen.brushCosts[i].text=AirtistApprovedTheme.Current!=null?costs[i].ToString():$"{costs[i]} / {hintCount}";
            }
            bool video=continuationAds!=null && continuationAds.IsReady;
            gameplayScreen.brushes[3].interactable=available && !exact && video;
            if(gameplayScreen.brushCosts!=null && gameplayScreen.brushCosts.Length>3 && gameplayScreen.brushCosts[3]!=null)
                gameplayScreen.brushCosts[3].text=AirtistApprovedTheme.Current!=null?(video?"▶":"—"):(video?"Видео":"Нет видео");
            gameplayScreen.mainTool.interactable=galleryOpen && !adPending;
            gameplayScreen.markTool.interactable=galleryOpen && !adPending;
            gameplayScreen.zoomTool.interactable=galleryOpen && !adPending;
            gameplayScreen.help.interactable=!adPending;
            var stock=gameplayScreen.transform.Find("HintStock")?.GetComponent<TMPro.TMP_Text>();
            if(stock!=null)stock.text="Запас:\n"+hintCount;
        }
        private void RefreshAreaHintVisual()
        {
            if(gameplayScreen==null) return;
            bool visible=ActiveHint(areaHintTargets[selectedChapter]) && !IsChapterComplete(selectedChapter);
            if(areaHintVisual==null && visible)
            {
                var go=new GameObject("AreaBrushHighlight",typeof(RectTransform),typeof(UnityEngine.UI.Image));
                areaHintVisual=(RectTransform)go.transform;areaHintVisual.SetParent(gameplayScreen.artworkContent,false);
                var image=go.GetComponent<UnityEngine.UI.Image>(); image.color=new Color(.99f,.80f,.26f,.20f);image.raycastTarget=false;
            }
            if(areaHintVisual==null) return;
            areaHintVisual.gameObject.SetActive(visible);
            if(!visible) return;
            var point=HintPoint(areaHintTargets[selectedChapter]-1);
            var cell=new Vector2(Mathf.Min(2,Mathf.Floor(point.x*3)),Mathf.Min(2,Mathf.Floor(point.y*3)))/3;
            areaHintVisual.anchorMin=cell;areaHintVisual.anchorMax=cell+Vector2.one/3;
            areaHintVisual.offsetMin=areaHintVisual.offsetMax=Vector2.zero;
        }
        private void ShowToolHelp()
        {
            var theme=AirtistApprovedTheme.Current;
            if(theme!=null && gameplayScreen!=null)
            {
                var root=(RectTransform)gameplayScreen.transform;
                var existing=root.Find("ApprovedToolHelp");
                if(existing!=null){existing.gameObject.SetActive(true);existing.SetAsLastSibling();return;}
                var panel=CreatePanel(root,"ApprovedToolHelp",AirtistApprovedTheme.Paper,Anchor.Center,Vector2.zero,new Vector2(1000,560));
                AirtistApprovedTheme.Rect(panel,.25f,.15f,.68f,.60f);theme.Surface(panel.GetComponent<UnityEngine.UI.Image>(),AirtistApprovedTheme.Paper);
                var text=CreateLabel(panel,"Проверка — ищет предмет и расходует энергию.\nПометка — оставляет временную заметку.\nОбзор — движение и зум без проверок.\n\nОбласть — показывает зону. Точно — находит предмет.\nУбрать — исключает предмет. Видео — помощь за ролик.\nЧисло у кисти — цена в подсказках.",25,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,new Vector2(900,440),TMPro.TextAlignmentOptions.Left);
                AirtistApprovedTheme.Rect(text.rectTransform,.06f,.12f,.88f,.80f);theme.Typography(text,root,25);
                var close=CreateButton(panel,"×",AirtistApprovedTheme.Paper,AirtistApprovedTheme.Ink,Anchor.TopRight,Vector2.zero,new Vector2(64,64),()=>panel.gameObject.SetActive(false),32);
                AirtistApprovedTheme.Rect((RectTransform)close.transform,.92f,.02f,.065f,.10f);theme.Button(close,AirtistApprovedTheme.Paper);
                AirtistApprovedTheme.Rect(GetButtonLabel(close).rectTransform,0,0,1,1);
                return;
            }
            galleryFeedback.text="Проверка — поиск. Пометка — заметки. Обзор — движение и зум, без проверок.";
            galleryHint.text="Кисти: область, точный предмет, убрать предмет, помощь за видео. Цена указана как расход / запас подсказок.";
        }
    }
}
