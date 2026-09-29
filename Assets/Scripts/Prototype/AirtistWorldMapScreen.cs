using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Airtist.Prototype
{
    public sealed class AirtistWorldMapScreen : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
    {
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform map;
        [SerializeField] private UnityEngine.UI.Button[] buttons;
        private float zoom = 1;
        private Vector2 previousViewport;
        public RectTransform MapContent => map;
        public RectTransform Viewport => viewport;
        public float Zoom => zoom;
        public UnityEngine.UI.Button[] Controls => buttons;
        public void UseUniversalNavigation()
        {
            for (int i = 0; i < 3; i++) buttons[i].gameObject.SetActive(false);
        }
        public void Configure(RectTransform view, RectTransform content, UnityEngine.UI.Button[] controls)
        { viewport = view; map = content; buttons = controls; }
        public void Bind(Action home, Action collection, Action profile)
        {
            // The map occupies all space below shared navigation, without the old wide frame.
            viewport.anchorMin=Vector2.zero; viewport.anchorMax=new Vector2(1,.895f);
            viewport.offsetMin=viewport.offsetMax=Vector2.zero;
            var frame=transform.Find("WoodenFrame");
            if(frame!=null) frame.gameObject.SetActive(false);
            viewport.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            viewport.GetComponent<UnityEngine.UI.Image>().color=new Color(.68f,.48f,.28f);
            buttons[0].onClick.AddListener(() => home());
            buttons[1].onClick.AddListener(() => collection());
            buttons[2].onClick.AddListener(() => profile());
            buttons[3].onClick.AddListener(() => SetZoom(zoom - .25f));
            buttons[4].onClick.AddListener(() => SetZoom(zoom + .25f));
            SetZoom(1);
        }
        private void SetZoom(float value)
        {
            zoom = Mathf.Clamp(value, 1, 6);
            map.localScale = Vector3.one * zoom;
            Clamp();
            buttons[3].interactable = zoom > 1;
            buttons[4].interactable = zoom < 6;
        }
        public void FocusOn(Vector2 location,float targetZoom)
        {
            RefreshLayout();SetZoom(targetZoom);
            map.anchoredPosition=-Vector2.Scale(location-map.pivot,map.rect.size)*zoom;
            Clamp();
        }
        public void OnBeginDrag(PointerEventData e) { e.eligibleForClick=false; }
        public void OnDrag(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, e.position, e.pressEventCamera, out var current);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, e.position - e.delta, e.pressEventCamera, out var previous);
            map.anchoredPosition += current - previous;
            Clamp();
        }
        public void OnScroll(PointerEventData e) => SetZoom(zoom + e.scrollDelta.y * .15f);
        private void LateUpdate() => RefreshLayout();
        public void RefreshLayout()
        {
            if(map==null || viewport==null) return;
            // The atlas itself reaches the physical screen edges. Zoom controls stay in SafeArea.
            var canvas=GetComponentInParent<Canvas>();
            if(canvas!=null && viewport.parent is RectTransform parent)
            {
                var canvasRect=(RectTransform)canvas.transform;
                Vector3 lower=parent.InverseTransformPoint(canvasRect.TransformPoint(new Vector3(canvasRect.rect.xMin,canvasRect.rect.yMin,0)));
                Vector3 upper=parent.InverseTransformPoint(canvasRect.TransformPoint(new Vector3(canvasRect.rect.xMax,canvasRect.rect.yMax,0)));
                viewport.offsetMin=new Vector2(lower.x-parent.rect.xMin,lower.y-parent.rect.yMin);
                viewport.offsetMax=new Vector2(upper.x-parent.rect.xMax,0);
            }
            if((previousViewport-viewport.rect.size).sqrMagnitude>.01f)
            {
                previousViewport=viewport.rect.size;
                var image=map.GetComponent<UnityEngine.UI.Image>();
                if(image!=null && image.sprite!=null && previousViewport.x>0 && previousViewport.y>0)
                {
                    float aspect=image.sprite.rect.width/image.sprite.rect.height;
                    // Cover the viewport; even at minimum zoom the cropped direction can pan.
                    float width=Mathf.Max(previousViewport.x,previousViewport.y*aspect);
                    map.anchorMin=map.anchorMax=Vector2.one*.5f;
                    map.sizeDelta=new Vector2(width,width/aspect);
                }
            }
            Clamp();
        }
        private void Clamp()
        {
            Vector2 limit = Vector2.Max(Vector2.zero,(map.rect.size*zoom-viewport.rect.size)/2);
            map.anchoredPosition = new Vector2(Mathf.Clamp(map.anchoredPosition.x, -limit.x, limit.x), Mathf.Clamp(map.anchoredPosition.y, -limit.y, limit.y));
        }
    }
}
