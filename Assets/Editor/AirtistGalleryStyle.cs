using System;
using UnityEditor;
using UnityEngine;
using TMPro;

namespace Airtist.Prototype.Editor
{
    public static class AirtistGalleryStyle
    {
        private static readonly Color Paper = new Color(.99f,.96f,.88f);
        private static readonly Color Sage = new Color(.80f,.87f,.78f);
        public static string Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before editing gallery.");
            foreach(var path in new[]{AirtistMyGalleryBuilder.CardPath,AirtistMyGalleryBuilder.ScreenPath})
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach(var card in root.GetComponentsInChildren<AirtistGalleryCard>(true)) StyleCard(card);
                    var view=root.GetComponent<AirtistMyGalleryScreen>();
                    if(view!=null)
                    {
                        Flat(root.transform.Find("ProfilePanel"),Paper);
                        foreach(var button in view.museumFilters) Flat(button.transform,Sage);
                        if(view.nextPainting!=null) Flat(view.nextPainting.transform,new Color(.91f,.71f,.57f));
                        var grid=view.content.GetComponent<UnityEngine.UI.GridLayoutGroup>();
                        grid.spacing=new Vector2(24,28);
                        grid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                        grid.constraintCount=3;
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally {PrefabUtility.UnloadPrefabContents(root);}
            }
            AssetDatabase.SaveAssets();
            return "Gallery and card styled in place. Three columns and scroll references preserved. Rating slots prepared, not awarded. No Play/build.";
        }
        private static void StyleCard(AirtistGalleryCard card)
        {
            foreach(string name in new[]{"GoldFrame","PaperMat","CaptionPanel"})
            {
                var layer=card.transform.Find(name);
                if(layer!=null) layer.gameObject.SetActive(false);
            }
            var background=card.GetComponent<UnityEngine.UI.Image>() ?? card.gameObject.AddComponent<UnityEngine.UI.Image>();
            background.sprite=null; background.color=Paper; background.raycastTarget=false;
            card.artwork.preserveAspect=true;
            Place(card.artwork.rectTransform,.04f,.38f,.96f,.97f);
            Place(card.title.rectTransform,.05f,.28f,.95f,.37f);
            Place(card.artist.rectTransform,.05f,.22f,.95f,.28f);
            Place(card.state.rectTransform,.05f,.15f,.95f,.21f);
            Place((RectTransform)card.action.transform,.06f,.025f,.94f,.13f);
            Flat(card.action.transform,Sage);
            Flat(card.locked.transform,new Color(.94f,.90f,.81f,.95f));
            foreach(var label in card.GetComponentsInChildren<TMP_Text>(true))
                label.color=new Color(.09f,.20f,.24f);
            // No ratings exist in the save schema yet. Do not invent earned stars for old saves.
            if(card.transform.Find("RatingStars")==null)
            {
                var row=new GameObject("RatingStars",typeof(RectTransform));
                row.transform.SetParent(card.transform,false);
                Place((RectTransform)row.transform,.31f,.15f,.69f,.21f);
                for(int i=0;i<3;i++)
                {
                    var star=new GameObject("Star"+(i+1),typeof(RectTransform),typeof(CanvasRenderer),typeof(AirtistRatingStar));
                    star.transform.SetParent(row.transform,false);
                    Place((RectTransform)star.transform,i/3f,0,(i+1)/3f,1);
                    star.GetComponent<AirtistRatingStar>().raycastTarget=false;
                }
                row.SetActive(false);
            }
        }
        private static void Flat(Transform target,Color color)
        {
            var image=target!=null?target.GetComponent<UnityEngine.UI.Image>():null;
            if(image==null) return;
            image.sprite=null; image.type=UnityEngine.UI.Image.Type.Simple; image.color=color;
        }
        private static void Place(RectTransform rect,float x0,float y0,float x1,float y1)
        {
            rect.anchorMin=new Vector2(x0,y0); rect.anchorMax=new Vector2(x1,y1);
            rect.offsetMin=rect.offsetMax=Vector2.zero;
        }
    }
}
