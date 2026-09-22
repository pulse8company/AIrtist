using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        private UnityEngine.UI.Image fullBleedPaper;
        private GameObject fullBleedHome;
        private readonly System.Collections.Generic.HashSet<Page> studioBackgroundPages = new System.Collections.Generic.HashSet<Page>();
        private void ApplyFullscreenPresentation()
        {
            if(myGallery!=null) myGallery.ratingMask=index=>Mathf.Max(restorations[index]?.bestMask ?? 0,chapterCollected[index]?1:0);
            if(museumGallery!=null) museumGallery.ratingMask=index=>Mathf.Max(restorations[index]?.bestMask ?? 0,chapterCollected[index]?1:0);
            // Painting and map content retain their own aspect/zoom logic; only their UI viewport grows.
            if(gameplayScreen!=null) AirtistFlexibleArtboard.Fill((RectTransform)gameplayScreen.transform);
            if(myGallery!=null) AirtistFlexibleArtboard.Fill((RectTransform)myGallery.transform);
            if(museumGallery!=null) AirtistFlexibleArtboard.Fill((RectTransform)museumGallery.transform);
            foreach(var map in GetComponentsInChildren<AirtistWorldMapScreen>(true))
                AirtistFlexibleArtboard.Fill((RectTransform)map.transform);
            foreach(var store in GetComponentsInChildren<AirtistStoreScreen>(true))
                AirtistFlexibleArtboard.Fill((RectTransform)store.transform);
            if(universalHeader!=null) AirtistFlexibleArtboard.Fill(universalHeader);

            // Full-bleed background under the notch/gesture inset. Controls remain in SafeArea.
            var canvas=GetComponentInParent<Canvas>();
            if(canvas!=null && canvas.transform.Find("FullBleedPaper")==null)
            {
                var go=new GameObject("FullBleedPaper",typeof(RectTransform),typeof(UnityEngine.UI.Image));
                var rect=go.GetComponent<RectTransform>(); rect.SetParent(canvas.transform,false); Stretch(rect);
                rect.SetAsFirstSibling();
                fullBleedPaper=go.GetComponent<UnityEngine.UI.Image>(); fullBleedPaper.color=Parchment; fullBleedPaper.raycastTarget=false;
                if(homeBackground!=null)
                {
                    var backdrop=CreateImage(rect,"HomeBackdrop",homeBackground,Color.white,
                        Anchor.Stretch,Vector2.zero,Vector2.zero,false);
                    backdrop.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
                    var fit=backdrop.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                    fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
                    fit.aspectRatio=homeBackground.rect.width/homeBackground.rect.height;
                    fullBleedHome=backdrop.gameObject;
                }
            }
            if(illustratedHome!=null)
            {
                AirtistFlexibleArtboard.Fill((RectTransform)illustratedHome.transform);
                illustratedHome.ApplyFlexiblePresentation(roundedSprite,homeBackground);
                if(fullBleedHome!=null)
                {
                    pages[Page.Home].GetComponent<UnityEngine.UI.Image>().color=Color.clear;
                    studioBackgroundPages.Add(Page.Home);
                }
            }
            // A single backdrop spans the real screen, while each page's controls stay safe.
            foreach(var pair in pages)
            {
                if(fullBleedHome==null) break;
                foreach(var image in pair.Value.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                {
                    if(image.name!="ParisStudioBackground") continue;
                    image.enabled=false;
                    pair.Value.GetComponent<UnityEngine.UI.Image>().color=Color.clear;
                    studioBackgroundPages.Add(pair.Key);
                }
            }
            if(gameplayScreen!=null)
            {
                AirtistPaintingFrame.Attach(gameplayScreen.painting);
                pages[Page.Gallery].GetComponent<UnityEngine.UI.Image>().color=new Color(.86f,.91f,.90f);
                TintGameplayPanel("CharacterPanel",new Color(.95f,.84f,.74f));
                TintGameplayPanel("PaintingFrame",new Color(.97f,.94f,.85f));
                TintGameplayPanel("ArtworkViewport",new Color(.80f,.87f,.89f));
                TintGameplayPanel("ToolDock",new Color(.82f,.88f,.79f));
                PreservePortraitAspect(gameplayScreen.transform.Find("AmeliePortrait"));
                if(museumSearchBackground!=null)
                {
                    var viewport=gameplayScreen.viewport;
                    var museum=CreateImage(viewport,"MuseumRoomBackdrop",museumSearchBackground,Color.white,Anchor.Stretch,Vector2.zero,Vector2.zero,false);
                    museum.SetAsFirstSibling(); museum.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
                    var fit=museum.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                    fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
                    fit.aspectRatio=museumSearchBackground.rect.width/museumSearchBackground.rect.height;
                }
            }
            Canvas.ForceUpdateCanvases();
        }

        private void UpdateFullscreenBackground(Page target)
        {
            if(fullBleedHome!=null) fullBleedHome.SetActive(studioBackgroundPages.Contains(target));
            if(fullBleedPaper!=null && pages.TryGetValue(target,out var page))
            {
                var background=page.GetComponent<UnityEngine.UI.Image>();
                fullBleedPaper.color=background!=null ? background.color : Parchment;
            }
        }

        private void TintGameplayPanel(string name,Color color)
        {
            var child=gameplayScreen.transform.Find(name);
            var image=child!=null?child.GetComponent<UnityEngine.UI.Image>():null;
            if(image==null) return;
            image.sprite=null; image.color=color;
        }

        private static void PreservePortraitAspect(Transform portrait)
        {
            var raw=portrait!=null?portrait.GetComponent<UnityEngine.UI.RawImage>():null;
            if(raw==null || raw.texture==null) return;
            var rect=(RectTransform)portrait;
            var slot=new GameObject("AmeliePortraitSlot",typeof(RectTransform)).GetComponent<RectTransform>();
            slot.SetParent(portrait.parent,false); slot.SetSiblingIndex(portrait.GetSiblingIndex());
            slot.anchorMin=rect.anchorMin; slot.anchorMax=rect.anchorMax;
            slot.offsetMin=rect.offsetMin; slot.offsetMax=rect.offsetMax;
            rect.SetParent(slot,false);
            var fit=portrait.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
            fit.aspectRatio=raw.texture.width*raw.uvRect.width/(raw.texture.height*raw.uvRect.height);
        }
    }
}
