using UnityEngine;

namespace Airtist.Prototype
{
    // Decorative only: never intercepts a tap on the highlighted control or painting.
    public sealed class AirtistTutorialCue : MonoBehaviour
    {
        public RectTransform target;
        private RectTransform rect;
        private readonly Vector3[] corners=new Vector3[4];
        private void LateUpdate()
        {
            if(target==null)return;
            if(rect==null)rect=(RectTransform)transform;
            target.GetWorldCorners(corners);
            var parent=(RectTransform)rect.parent;
            var a=parent.InverseTransformPoint(corners[0]);
            var b=parent.InverseTransformPoint(corners[2]);
            rect.position=target.TransformPoint(target.rect.center);
            rect.sizeDelta=new Vector2(Mathf.Abs(b.x-a.x)+10,Mathf.Abs(b.y-a.y)+10);
        }
    }
}
