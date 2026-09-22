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
            viewport.anchorMin=Vector2.zero; viewport.anchorMax=new Vector2(1,831f/941);
            viewport.offsetMin=viewport.offsetMax=Vector2.zero;
            var frame=transform.Find("WoodenFrame");
            if(frame!=null) frame.gameObject.SetActive(false);
            viewport.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            buttons[0].onClick.AddListener(() => home());
            buttons[1].onClick.AddListener(() => collection());
            buttons[2].onClick.AddListener(() => profile());
            buttons[3].onClick.AddListener(() => SetZoom(zoom - .25f));
            buttons[4].onClick.AddListener(() => SetZoom(zoom + .25f));
            SetZoom(1);
        }
        private void SetZoom(float value)
        {
            zoom = Mathf.Clamp(value, 1, 3);
            map.localScale = Vector3.one * zoom;
            Clamp();
            buttons[3].interactable = zoom > 1;
            buttons[4].interactable = zoom < 3;
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
        private void LateUpdate()
        {
            if(map==null || viewport==null) return;
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
