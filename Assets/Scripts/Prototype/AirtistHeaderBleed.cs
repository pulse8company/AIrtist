using UnityEngine;

namespace Airtist.Prototype
{
    // Background only: extend behind the notch without moving any touch targets out of SafeArea.
    public sealed class AirtistHeaderBleed : MonoBehaviour
    {
        public RectTransform source;
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            if(source==null)return;
            var rect=(RectTransform)transform;var parent=(RectTransform)rect.parent;
            var canvas=GetComponentInParent<Canvas>();if(canvas==null)return;
            var canvasRect=(RectTransform)canvas.transform;
            Vector3 left=parent.InverseTransformPoint(canvasRect.TransformPoint(new Vector3(canvasRect.rect.xMin,canvasRect.rect.yMax,0)));
            Vector3 right=parent.InverseTransformPoint(canvasRect.TransformPoint(new Vector3(canvasRect.rect.xMax,canvasRect.rect.yMax,0)));
            float bottom=parent.InverseTransformPoint(source.TransformPoint(new Vector3(0,source.rect.yMin,0))).y;
            rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);
            rect.anchoredPosition=new Vector2((left.x+right.x)/2-parent.rect.center.x,left.y-parent.rect.yMax);
            rect.sizeDelta=new Vector2(right.x-left.x-parent.rect.width,Mathf.Max(0,left.y-bottom));
        }
    }
}
