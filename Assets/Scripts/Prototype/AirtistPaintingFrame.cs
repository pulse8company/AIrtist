using UnityEngine;

namespace Airtist.Prototype
{
    /// <summary>A narrow matte frame follows the actual image, not its letterboxed UI slot.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AirtistPaintingFrame : UnityEngine.UI.MaskableGraphic
    {
        public UnityEngine.UI.Image artwork;
        [Range(1,8)] public float thickness=3;
        private Sprite previousSprite;
        public static void Attach(UnityEngine.UI.Image image)
        {
            if(image==null || image.transform.Find("SlimWoodFrame")!=null) return;
            var go=new GameObject("SlimWoodFrame",typeof(RectTransform),typeof(CanvasRenderer),typeof(AirtistPaintingFrame));
            var rect=(RectTransform)go.transform; rect.SetParent(image.transform,false);
            rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero;
            var frame=go.GetComponent<AirtistPaintingFrame>(); frame.artwork=image;
            frame.color=new Color(.47f,.32f,.18f); frame.raycastTarget=false;
        }
        private void LateUpdate()
        {
            var sprite=artwork!=null?artwork.sprite:null;
            if(sprite!=previousSprite) {previousSprite=sprite;SetVerticesDirty();}
        }
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear(); if(artwork==null || artwork.sprite==null) return;
            var area=rectTransform.rect;
            if(area.width<=0 || area.height<=0) return;
            if(artwork.preserveAspect)
            {
                float aspect=artwork.sprite.rect.width/artwork.sprite.rect.height;
                float width=Mathf.Min(area.width,area.height*aspect),height=width/aspect;
                area=new Rect(area.center-new Vector2(width,height)*.5f,new Vector2(width,height));
            }
            float t=thickness;
            Quad(area.xMin-t,area.yMin-t,area.xMax+t,area.yMin);
            Quad(area.xMin-t,area.yMax,area.xMax+t,area.yMax+t);
            Quad(area.xMin-t,area.yMin,area.xMin,area.yMax);
            Quad(area.xMax,area.yMin,area.xMax+t,area.yMax);
            void Quad(float x0,float y0,float x1,float y1)
            {
                int v=mesh.currentVertCount;
                mesh.AddVert(new Vector3(x0,y0),color,Vector2.zero); mesh.AddVert(new Vector3(x0,y1),color,Vector2.zero);
                mesh.AddVert(new Vector3(x1,y1),color,Vector2.zero); mesh.AddVert(new Vector3(x1,y0),color,Vector2.zero);
                mesh.AddTriangle(v,v+1,v+2); mesh.AddTriangle(v+2,v+3,v);
            }
        }
    }
}
