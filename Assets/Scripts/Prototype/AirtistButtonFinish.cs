using UnityEngine;

namespace Airtist.Prototype
{
    // A light, resolution-independent enamel finish. It never participates in hit testing.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AirtistButtonFinish : UnityEngine.UI.MaskableGraphic
    {
        private UnityEngine.UI.Button button;
        private bool available=true;
        private float transition=1;
        private const int Steps=10,Count=Steps*4;

        public static void Attach(UnityEngine.UI.Button target)
        {
            if(target==null || target.image==null)return;
            var child=target.transform.Find("EnamelFinish");
            var finish=child!=null?child.GetComponent<AirtistButtonFinish>():null;
            if(finish==null)
            {
                var go=new GameObject("EnamelFinish",typeof(RectTransform),typeof(CanvasRenderer),typeof(AirtistButtonFinish));
                go.transform.SetParent(target.transform,false);finish=go.GetComponent<AirtistButtonFinish>();
            }
            finish.button=target;finish.raycastTarget=false;
            var rect=finish.rectTransform;
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            // Keep labels and painted icons above the finish.
            rect.SetAsFirstSibling();finish.Refresh();
        }

        private void LateUpdate(){Refresh();}
        public void Refresh()
        {
            if(button==null)return;
            bool next=button.IsInteractable();
            float tint=button.targetGraphic!=null?button.targetGraphic.canvasRenderer.GetColor().r:1;
            if(available==next && Mathf.Abs(transition-tint)<.005f)return;
            available=next;transition=tint;SetVerticesDirty();
        }

        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();var rect=rectTransform.rect;
            if(rect.width<5 || rect.height<5)return;
            float scale=Mathf.Clamp(rect.height/72f,.6f,1.5f);
            float radius=Mathf.Min(14f*scale,Mathf.Min(rect.width,rect.height)*.2f);
            float intensity=(available?1f:.20f)*Mathf.Clamp01(transition);
            // Broad upper sheen, a darker lower bevel and a fine, warm inner rim.
            Fill(3f*scale,radius);
            Ring(2.3f*scale,1.2f*scale,radius,false);
            Ring(4.2f*scale,.8f*scale,radius-2f*scale,true);

            Vector2 Point(int index,float inset,float r)
            {
                var inner=new Rect(rect.x+inset,rect.y+inset,Mathf.Max(1,rect.width-inset*2),Mathf.Max(1,rect.height-inset*2));
                r=Mathf.Clamp(r,0,Mathf.Min(inner.width,inner.height)*.5f);
                int corner=index/Steps;float angle=(corner*90f+(index%Steps)*90f/(Steps-1))*Mathf.Deg2Rad;
                var center=new Vector2(corner==0||corner==3?inner.xMax-r:inner.xMin+r,corner<2?inner.yMax-r:inner.yMin+r);
                return center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r;
            }
            Color Gloss(Vector2 p)
            {
                float y=Mathf.InverseLerp(rect.yMin,rect.yMax,p.y);
                return new Color(1f,.99f,.91f,Mathf.Clamp01((y-.46f)/.54f)*.24f*intensity);
            }
            void Fill(float inset,float r)
            {
                int start=mesh.currentVertCount;
                mesh.AddVert(rect.center,Gloss(rect.center),Vector2.zero);
                for(int i=0;i<Count;i++){var p=Point(i,inset,r);mesh.AddVert(p,Gloss(p),Vector2.zero);}
                for(int i=0;i<Count;i++)mesh.AddTriangle(start,start+1+i,start+1+(i+1)%Count);
            }
            void Ring(float inset,float width,float r,bool inner)
            {
                int start=mesh.currentVertCount;
                for(int i=0;i<Count;i++)
                {
                    var p=Point(i,inset,r);float y=Mathf.InverseLerp(rect.yMin,rect.yMax,p.y);
                    var shade=inner?new Color(1,.97f,.80f,.32f*intensity):Color.Lerp(new Color(.20f,.14f,.07f,.30f*intensity),new Color(1,1,.91f,.60f*intensity),y);
                    mesh.AddVert(p,shade,Vector2.zero);mesh.AddVert(Point(i,inset+width,Mathf.Max(0,r-width)),shade,Vector2.zero);
                }
                for(int i=0;i<Count;i++){int a=start+i*2,b=start+((i+1)%Count)*2;mesh.AddTriangle(a,b,a+1);mesh.AddTriangle(a+1,b,b+1);}
            }
        }
    }
}
