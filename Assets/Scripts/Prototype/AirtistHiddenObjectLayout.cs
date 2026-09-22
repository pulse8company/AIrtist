using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistHiddenObjectLayout : MonoBehaviour
    {
        public RectTransform painting, artboard, visual;
        public float widthFraction=.08f;
        public float aspect=1;
        private RectTransform hit;
        private void Awake() => hit=(RectTransform)transform;
        private void LateUpdate()
        {
            if(painting==null || visual==null) return;
            if(hit==null) hit=(RectTransform)transform;
            float scale=artboard!=null ? AirtistFlexibleArtboard.Scale(artboard) : 1;
            float w=painting.rect.width*widthFraction;
            float h=w/Mathf.Max(.1f,aspect);
            visual.sizeDelta=new Vector2(w,h);
            // Larger than the visible object, without enlarging the illustration itself.
            hit.sizeDelta=new Vector2(Mathf.Max(72*scale,w+18*scale),Mathf.Max(64*scale,h+18*scale));
        }
    }
}
