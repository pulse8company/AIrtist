using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        [SerializeField] private AirtistGameplayScreen gameplayPrefab;
        private AirtistGameplayScreen gameplayScreen;
        public void ConfigureGameplay(AirtistGameplayScreen prefab) => gameplayPrefab = prefab;

        private void BuildEditableGameplay()
        {
            var page = CreatePage(Page.Gallery, "Gallery");
            gameplayScreen = Instantiate(gameplayPrefab, page, false);
            attemptHud = gameplayScreen.GetComponent<AirtistAttemptHud>();
            if(attemptHud!=null)
            {
                attemptHud.restart.onClick.AddListener(RestartAttempt);
                attemptHud.home.onClick.AddListener(()=>Show(Page.Home));
                if(attemptHud.close!=null) attemptHud.close.onClick.AddListener(()=>Show(Page.Home));
                attemptHud.bonusContinue.onClick.AddListener(ContinueWithBonus);
                attemptHud.rewardedContinue.onClick.AddListener(ContinueWithVideo);
            }
            galleryTitle = gameplayScreen.title;
            galleryDescription = gameplayScreen.description;
            galleryFeedback = gameplayScreen.feedback;
            galleryHint = gameplayScreen.hint;
            galleryTargetProgress = gameplayScreen.progress;
            galleryHintButton = gameplayScreen.help;
            galleryHintButtonLabel = GetButtonLabel(galleryHintButton);
            galleryPainting = gameplayScreen.painting;
            galleryPainting.sprite = GetChapterArtwork(selectedChapter);
            galleryPainting.gameObject.AddComponent<AirtistArtworkPointer>().clicked=e=>HandleArtworkPointer(e,selectedChapter,-1);
            galleryPanZoom = gameplayScreen.viewport.gameObject.AddComponent<PanZoomArtwork>();
            galleryPanZoom.Configure(gameplayScreen.viewport, gameplayScreen.artworkContent);
            galleryPanZoom.EnableViewportFit();
            Canvas.ForceUpdateCanvases();
            galleryPanZoom.SetArtworkSize(CalculateArtworkDisplaySize(galleryPainting.sprite));
            for (int c = 0; c < Chapters.Length; c++)
                for (int a = 0; a < Chapters[c].Artifacts.Length; a++)
                    galleryArtifactTargets.Add(CreateArtifactTarget(gameplayScreen.artworkContent, c, a, Chapters[c].Artifacts[a]));
            gameplayScreen.mainTool.onClick.AddListener(()=>SelectWorkingTool(false));
            if(gameplayScreen.markTool!=null) gameplayScreen.markTool.onClick.AddListener(()=>SelectWorkingTool(true));
            if(gameplayScreen.zoomTool!=null) gameplayScreen.zoomTool.onClick.AddListener(SelectOverview);
            gameplayScreen.help.onClick.AddListener(ShowToolHelp);
            gameplayScreen.brushRequested=UseBonusBrush;
            for(int i=0;i<gameplayScreen.brushes.Length;i++)
            {
                int index=i;
                gameplayScreen.brushes[i].onClick=new UnityEngine.UI.Button.ButtonClickedEvent();
                gameplayScreen.brushes[i].onClick.AddListener(()=>UseBonusBrush(index));
            }
            gameplayScreen.viewport.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            SelectWorkingTool(false);
        }
    }
}
