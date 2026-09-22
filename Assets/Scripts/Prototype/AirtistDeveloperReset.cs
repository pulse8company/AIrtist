using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private RectTransform developerResetRoot;
        private RectTransform developerResetButton;
        private GameObject developerResetConfirmation;
        private Sprite developerResetCircle;
        private Texture2D developerResetTexture;
#endif

        private void BuildDeveloperReset()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            developerResetRoot=new GameObject("DeveloperReset",typeof(RectTransform)).GetComponent<RectTransform>();
            developerResetRoot.SetParent(transform,false); Stretch(developerResetRoot);
            var button=CreateButton(developerResetRoot,"DEV",new Color(.82f,.08f,.10f),Color.white,
                Anchor.BottomRight,new Vector2(-24,24),new Vector2(88,88),OpenDeveloperReset,22);
            developerResetButton=(RectTransform)button.transform;
            developerResetTexture=new Texture2D(64,64,TextureFormat.RGBA32,false);
            var pixels=new Color[64*64];
            for(int y=0;y<64;y++) for(int x=0;x<64;x++)
                pixels[y*64+x]=new Color(1,1,1,Mathf.Clamp01(31.5f-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))));
            developerResetTexture.SetPixels(pixels); developerResetTexture.Apply();
            developerResetCircle=Sprite.Create(developerResetTexture,new Rect(0,0,64,64),Vector2.one*.5f);
            var image=button.GetComponent<UnityEngine.UI.Image>();
            image.sprite=developerResetCircle; image.type=UnityEngine.UI.Image.Type.Simple; image.raycastPadding=Vector4.zero;
            var shade=CreatePanel(developerResetRoot,"ResetConfirmation",new Color(0,0,0,.8f),Anchor.Stretch,Vector2.zero,Vector2.zero);
            Stretch(shade);
            var dialog=CreatePanel(shade,"Dialog",Cream,Anchor.Center,Vector2.zero,new Vector2(700,330));
            CreateLabel(dialog,"Сбросить прогресс?",32,Ink,Anchor.Top,new Vector2(0,-30),new Vector2(620,50),TextAlignmentOptions.Center);
            CreateLabel(dialog,"Картины и находки — с начала.\nЭнергия: 100 · Монеты: 0 · Подсказки: 3",23,Ink,Anchor.Center,new Vector2(0,10),new Vector2(620,100),TextAlignmentOptions.Center);
            CreateButton(dialog,"Отмена",Teal,Color.white,Anchor.Bottom,new Vector2(-165,30),new Vector2(280,64),()=>developerResetConfirmation.SetActive(false));
            CreateButton(dialog,"Сбросить",Coral,Color.white,Anchor.Bottom,new Vector2(165,30),new Vector2(280,64),ConfirmDeveloperReset);
            developerResetConfirmation=shade.gameObject; developerResetConfirmation.SetActive(false);
#endif
        }

        private void BringDeveloperResetToFront()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(developerResetRoot!=null) developerResetRoot.SetAsLastSibling();
            if(developerResetButton!=null)
            {
                // Use the fitted landscape artboard, not the outer window/letterbox.
                var parent=galleryOpen && gameplayScreen!=null ? (RectTransform)gameplayScreen.transform : developerResetRoot;
                developerResetButton.SetParent(parent,false);
                SetAnchor(developerResetButton,Anchor.BottomRight,new Vector2(-24,galleryOpen?230:24),new Vector2(88,88));
                developerResetButton.SetAsLastSibling();
            }
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OpenDeveloperReset()
        {
            if(adPending) return;
            Show(Page.Home); // Pause the attempt before showing the confirmation.
            developerResetConfirmation.SetActive(true);
        }

        private void ConfirmDeveloperReset()
        {
            if(adPending) return;
            AirtistProgress.ResetForDevelopment();
            for(int i=0;i<artifactFound.Length;i++) Array.Clear(artifactFound[i],0,artifactFound[i].Length);
            Array.Clear(chapterCollected,0,chapterCollected.Length);
            Array.Clear(hintUsedForChapter,0,hintUsedForChapter.Length);
            ClearTemporaryMarks(); LoadProgress();
            markingMode=false; SelectWorkingTool(false); galleryPanZoom?.ResetView();
            developerResetConfirmation.SetActive(false);
            UpdateProgressLabels(); Show(Page.Home);
        }
#endif
    }
}
