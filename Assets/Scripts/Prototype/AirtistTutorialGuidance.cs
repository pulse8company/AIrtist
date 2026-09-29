using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        private AirtistTutorialCue tutorialToolCue;
        private RectTransform tutorialObjectCue;

        private RectTransform MakeTutorialFrame(RectTransform parent,string name)
        {
            var root=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(parent,false);
            root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);
            for(int i=0;i<4;i++)
            {
                var line=new GameObject("Edge",typeof(RectTransform),typeof(UnityEngine.UI.Image));
                var r=(RectTransform)line.transform;r.SetParent(root,false);
                r.anchorMin=i<2?new Vector2(0,i):new Vector2(i-2,0);
                r.anchorMax=i<2?new Vector2(1,i):new Vector2(i-2,1);
                r.sizeDelta=i<2?new Vector2(0,4):new Vector2(4,0);r.anchoredPosition=Vector2.zero;
                var image=line.GetComponent<UnityEngine.UI.Image>();
                image.color=new Color(1,.76f,.22f);image.raycastTarget=false;
            }
            return root;
        }

        private void RefreshTutorialGuidance(bool visible)
        {
            if(!visible)
            {
                if(tutorialToolCue!=null)tutorialToolCue.gameObject.SetActive(false);
                if(tutorialObjectCue!=null)tutorialObjectCue.gameObject.SetActive(false);
                return;
            }
            UnityEngine.UI.Button button=tutorialStage==1?gameplayScreen.mainTool:
                tutorialStage==2?gameplayScreen.markTool:tutorialStage==3?gameplayScreen.zoomTool:
                tutorialStage>=4 && tutorialStage<=7?gameplayScreen.brushes[tutorialStage-4]:tutorialNext;
            if(tutorialStage==5 && exactHintTargets[0]==2)button=gameplayScreen.mainTool;
            if(tutorialToolCue==null)
                tutorialToolCue=MakeTutorialFrame((RectTransform)gameplayScreen.transform,"TutorialToolCue").gameObject.AddComponent<AirtistTutorialCue>();
            tutorialToolCue.target=(RectTransform)button.transform;
            tutorialToolCue.gameObject.SetActive(true);
            int item=tutorialStage==1?0:tutorialStage==5 && exactHintTargets[0]==2?1:-1;
            bool show=item>=0 && !artifactFound[0][item];
            if(show && tutorialObjectCue==null)
            {
                tutorialObjectCue=MakeTutorialFrame(gameplayScreen.artworkContent,"TutorialObjectCue");
                var arrow=IntroLabel(tutorialObjectCue,"TapHere","Здесь",0,-.45f,1,.35f,18);
                arrow.raycastTarget=false;
                arrow.color=AirtistApprovedTheme.Paper;
                arrow.outlineColor=AirtistApprovedTheme.Ink;arrow.outlineWidth=.18f;
            }
            if(tutorialObjectCue==null)return;
            tutorialObjectCue.gameObject.SetActive(show);
            if(show)
            {
                tutorialObjectCue.anchorMin=tutorialObjectCue.anchorMax=HintPoint(item);
                tutorialObjectCue.anchoredPosition=Vector2.zero;
                float w=gameplayScreen.artworkContent.rect.width;
                tutorialObjectCue.sizeDelta=new Vector2(w*.16f,w*.16f);
                tutorialObjectCue.SetAsLastSibling();
            }
        }
    }
}
