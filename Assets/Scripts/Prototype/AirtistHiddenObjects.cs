using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        [SerializeField] private AirtistHiddenObjectCatalog hiddenObjects;
        public void ConfigureHiddenObjects(AirtistHiddenObjectCatalog catalog) => hiddenObjects=catalog;

        private bool CreateIllustratedArtifact(RectTransform target,RectTransform parent,int chapter,int index)
        {
            var entry=hiddenObjects!=null ? hiddenObjects.Find(ChapterIds[chapter],index) : null;
            if(entry==null || entry.sprite==null) return false;
            target.anchorMin=target.anchorMax=entry.position;
            var visual=new GameObject("Object_"+entry.title,typeof(RectTransform),typeof(UnityEngine.UI.Image));
            var rect=visual.GetComponent<RectTransform>(); rect.SetParent(target,false);
            rect.anchorMin=rect.anchorMax=Vector2.one*.5f;
            rect.localRotation=Quaternion.Euler(0,0,entry.rotation);
            var image=visual.GetComponent<UnityEngine.UI.Image>(); image.sprite=entry.sprite; image.preserveAspect=true; image.raycastTarget=false;
            image.color=entry.tint;
            var layout=target.gameObject.AddComponent<AirtistHiddenObjectLayout>();
            layout.painting=parent; layout.artboard=gameplayScreen!=null?(RectTransform)gameplayScreen.transform:null;
            layout.visual=rect; layout.widthFraction=entry.widthFraction; layout.aspect=entry.sprite.rect.width/entry.sprite.rect.height;
            return true;
        }
    }
}
