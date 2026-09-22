using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        private bool markingMode;
        private bool overviewMode;
        private readonly List<RectTransform> temporaryMarks = new List<RectTransform>();

        private void SelectWorkingTool(bool mark)
        {
            markingMode=mark;
            overviewMode=false; galleryPanZoom?.SetNavigationMode(false);
            if(gameplayScreen==null) return;
            RefreshToolSelection();
            galleryFeedback.text=mark ? "Пометка бесплатна. Нажми ещё раз, чтобы убрать её." : $"Проверка: {SearchEnergyCost} энергии. Промах тоже расходует энергию.";
        }

        private void SelectOverview()
        {
            overviewMode=true; markingMode=false; galleryPanZoom?.SetNavigationMode(true);
            RefreshToolSelection();
            galleryFeedback.text="Обзор: двигай картину и своди/разводи два пальца. Проверки отключены.";
        }
        private void RefreshToolSelection()
        {
            if(gameplayScreen==null) return;
            var normal=new Color(.99f,.96f,.88f); var active=AirtistApprovedTheme.Current!=null?AirtistApprovedTheme.Teal:new Color(.73f,.84f,.80f);
            gameplayScreen.mainTool.image.color=!overviewMode && !markingMode?active:normal;
            gameplayScreen.markTool.image.color=markingMode?active:normal;
            gameplayScreen.zoomTool.image.color=overviewMode?active:normal;
            if(AirtistApprovedTheme.Current!=null)
                foreach(var b in new[]{gameplayScreen.mainTool,gameplayScreen.markTool,gameplayScreen.zoomTool})
                {var t=b.GetComponentInChildren<TMPro.TMP_Text>();if(t!=null)t.color=b.image.color==active?AirtistApprovedTheme.Paper:AirtistApprovedTheme.Ink;}
        }

        private void HandleArtworkPointer(PointerEventData e, int chapter, int artifact)
        {
            if(chapter!=selectedChapter) return;
            if(overviewMode || (galleryPanZoom!=null && galleryPanZoom.SuppressChecks)) return;
            TickAttemptClock();
            if(!galleryOpen || appPaused || appUnfocused || adPending || AttemptBlocked) return;
            if(!markingMode)
            {
                if(artifact>=0 && exactHintTargets[chapter]==artifact+1)
                {CompleteAssistedObject(artifact,true);return;}
                if(artifact>=0) FindArtworkArtifact(chapter,artifact); else RegisterIncorrectTap();
                return;
            }
            if(gameplayScreen==null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(gameplayScreen.artworkContent,e.position,e.pressEventCamera,out var point)) return;
            var content=gameplayScreen.artworkContent;
            var rect=content.rect;
            Vector2 uv=new Vector2((point.x-rect.xMin)/rect.width,(point.y-rect.yMin)/rect.height);
            for(int i=temporaryMarks.Count-1;i>=0;i--)
            {
                var m=temporaryMarks[i];
                Vector2 offset=Vector2.Scale(m.anchorMin-uv,rect.size);
                if(offset.sqrMagnitude<30*30)
                {
                    Destroy(m.gameObject); temporaryMarks.RemoveAt(i); return;
                }
            }
            if(temporaryMarks.Count>=8) { Destroy(temporaryMarks[0].gameObject); temporaryMarks.RemoveAt(0); }
            var go=new GameObject("TemporaryMark",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            var markRect=go.GetComponent<RectTransform>(); markRect.SetParent(content,false);
            markRect.anchorMin=markRect.anchorMax=uv; markRect.sizeDelta=new Vector2(26,26); markRect.anchoredPosition=Vector2.zero;
            var image=go.GetComponent<UnityEngine.UI.Image>(); image.sprite=roundedSprite; image.color=new Color(1,.75f,.2f,.65f); image.raycastTarget=false;
            temporaryMarks.Add(markRect);
        }

        private void ClearTemporaryMarks()
        {
            foreach(var mark in temporaryMarks) if(mark!=null) Destroy(mark.gameObject);
            temporaryMarks.Clear();
        }
    }
}
