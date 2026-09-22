using UnityEngine;

namespace Airtist.Prototype
{
    /// <summary>Editable vector UI star; no atlas or font glyph dependency.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AirtistRatingStar : UnityEngine.UI.MaskableGraphic
    {
        private Sprite Painted => AirtistApprovedTheme.Current!=null?AirtistApprovedTheme.Current.Icon(14):null;
        public override Texture mainTexture => Painted!=null?Painted.texture:base.mainTexture;
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            var rect=GetPixelAdjustedRect();
            var sprite=Painted;
            if(sprite!=null)
            {
                var uv=UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
                float side=Mathf.Min(rect.width,rect.height);var p=rect.center-Vector2.one*side/2;
                Color tint=color.r>.85f?Color.white:new Color(.52f,.52f,.48f,.65f);
                mesh.AddVert(p,tint,new Vector2(uv.x,uv.y));mesh.AddVert(p+new Vector2(0,side),tint,new Vector2(uv.x,uv.w));
                mesh.AddVert(p+Vector2.one*side,tint,new Vector2(uv.z,uv.w));mesh.AddVert(p+new Vector2(side,0),tint,new Vector2(uv.z,uv.y));
                mesh.AddTriangle(0,1,2);mesh.AddTriangle(0,2,3);return;
            }
            var center=rect.center;
            float radius=Mathf.Min(rect.width,rect.height)*.46f;
            mesh.AddVert(center,color,Vector2.zero);
            for(int i=0;i<10;i++)
            {
                float angle=(90-i*36)*Mathf.Deg2Rad;
                float r=radius*(i%2==0?1:.45f);
                mesh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r,color,Vector2.zero);
            }
            for(int i=0;i<10;i++) mesh.AddTriangle(0,i+1,(i+1)%10+1);
        }
    }
}
