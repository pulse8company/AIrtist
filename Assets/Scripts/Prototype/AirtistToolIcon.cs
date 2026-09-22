using UnityEngine;

namespace Airtist.Prototype
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AirtistToolIcon : UnityEngine.UI.MaskableGraphic
    {
        public int kind;
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            var rect=GetPixelAdjustedRect(); float size=Mathf.Min(rect.width,rect.height);
            Vector2 origin=rect.center-Vector2.one*size*.5f;
            Vector2 P(float x,float y)=>origin+new Vector2(x,y)*size;
            void Line(float x0,float y0,float x1,float y1,float width=.045f)
            {
                Vector2 a=P(x0,y0),b=P(x1,y1),d=(b-a).normalized;
                Vector2 n=new Vector2(-d.y,d.x)*size*width*.5f;
                int v=mesh.currentVertCount;
                mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);
                mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);
                mesh.AddTriangle(v,v+1,v+2);mesh.AddTriangle(v+2,v+3,v);
            }
            void Circle(float x,float y,float radius,bool dotted=false)
            {
                for(int i=0;i<32;i++)
                {
                    if(dotted && i%4>=2) continue;
                    float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;
                    Line(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius,.038f);
                }
            }
            if(kind==0) {Circle(.42f,.59f,.25f);Line(.6f,.4f,.84f,.15f,.09f);}
            else if(kind==1) {Circle(.5f,.64f,.22f);Line(.31f,.52f,.5f,.16f);Line(.69f,.52f,.5f,.16f);Circle(.5f,.64f,.055f);}
            else if(kind==2)
            {
                Line(.15f,.5f,.85f,.5f);Line(.5f,.15f,.5f,.85f);
                Line(.15f,.5f,.28f,.63f);Line(.15f,.5f,.28f,.37f);Line(.85f,.5f,.72f,.63f);Line(.85f,.5f,.72f,.37f);
                Line(.5f,.85f,.37f,.72f);Line(.5f,.85f,.63f,.72f);Line(.5f,.15f,.37f,.28f);Line(.5f,.15f,.63f,.28f);
            }
            else
            {
                // Brush stem and ferrule, plus a distinct ability symbol.
                Line(.18f,.15f,.4f,.43f,.13f);Line(.4f,.43f,.72f,.87f,.075f);
                Line(.30f,.32f,.42f,.40f,.08f);
                if(kind==3) Circle(.72f,.30f,.16f,true);
                else if(kind==4) {Circle(.73f,.3f,.14f);Line(.53f,.3f,.93f,.3f,.025f);Line(.73f,.1f,.73f,.5f,.025f);}
                else if(kind==5) {Circle(.73f,.3f,.16f);Line(.63f,.3f,.83f,.3f,.05f);}
                else {Line(.64f,.14f,.64f,.46f);Line(.64f,.46f,.90f,.30f);Line(.90f,.30f,.64f,.14f);}
            }
        }
    }
}
