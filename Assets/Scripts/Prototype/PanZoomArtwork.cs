using UnityEngine;
using UnityEngine.EventSystems;

namespace Airtist.Prototype
{
    /// <summary>
    /// Keeps a painting inside its gallery window while allowing a player to inspect it with
    /// drag and zoom controls. Pointer drag works for mouse and a single finger.
    /// </summary>
    public sealed class PanZoomArtwork : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
    {
        private const float MinimumScale = 1f;
        private const float MaximumScale = 2.4f;
        private const float ButtonZoomStep = 0.24f;

        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;

        private Canvas parentCanvas;
        private Vector2 artworkSize;
        private float currentScale = MinimumScale;
        private bool fitToViewport;
        private Vector2 lastViewportSize;
        private bool navigationMode, waitForRelease;
        private float previousPinchDistance, blockedUntil;
        private int pinchA=-1,pinchB=-1;
        public bool SuppressChecks => navigationMode || waitForRelease || Time.unscaledTime<blockedUntil;

        public void SetNavigationMode(bool enabled)
        {
            navigationMode=enabled; previousPinchDistance=0; pinchA=pinchB=-1;
            waitForRelease=true; blockedUntil=Time.unscaledTime+.2f;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var screen=UnityEngine.InputSystem.Touchscreen.current;
            int count=0,a=-1,b=-1; Vector2 first=default,second=default;
            if(screen!=null) foreach(var touch in screen.touches)
            {
                if(!touch.press.isPressed) continue;
                if(count==0) {first=touch.position.ReadValue();a=touch.touchId.ReadValue();}
                else if(count==1) {second=touch.position.ReadValue();b=touch.touchId.ReadValue();}
                count++;
            }
            if(count==0) waitForRelease=false;
            if(count>=2) {waitForRelease=true;blockedUntil=Time.unscaledTime+.2f;}
            Camera camera=parentCanvas!=null && parentCanvas.renderMode!=RenderMode.ScreenSpaceOverlay?parentCanvas.worldCamera:null;
            if(!navigationMode || count!=2 || viewport==null || content==null
                || !RectTransformUtility.RectangleContainsScreenPoint(viewport,first,camera)
                || !RectTransformUtility.RectangleContainsScreenPoint(viewport,second,camera))
            {previousPinchDistance=0;pinchA=pinchB=-1;return;}
            float distance=Vector2.Distance(first,second);
            if(pinchA==a && pinchB==b && previousPinchDistance>1)
            {
                float next=Mathf.Clamp(currentScale*distance/previousPinchDistance,MinimumScale,MaximumScale);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport,(first+second)*.5f,camera,out var center);
                content.anchoredPosition=center-(center-content.anchoredPosition)*(next/currentScale);
                currentScale=next; content.localScale=Vector3.one*currentScale; ClampPosition();
            }
            previousPinchDistance=distance;pinchA=a;pinchB=b;
#else
            waitForRelease=false;
#endif
        }

        public void FocusOn(Vector2 uv)
        {
            if(content==null) return;
            currentScale=Mathf.Max(1.5f,currentScale);content.localScale=Vector3.one*currentScale;
            content.anchoredPosition=-Vector2.Scale(uv-Vector2.one*.5f,artworkSize)*currentScale;
            ClampPosition();
        }

        public void EnableViewportFit()
        {
            fitToViewport = true;
            lastViewportSize = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (!fitToViewport || viewport == null || content == null || artworkSize.x <= 0 || artworkSize.y <= 0) return;
            var size = viewport.rect.size;
            if (size.x <= 8 || size.y <= 8 || (size - lastViewportSize).sqrMagnitude < .01f) return;
            lastViewportSize = size;
            float factor = Mathf.Min((size.x - 8) / artworkSize.x, (size.y - 8) / artworkSize.y);
            artworkSize *= factor;
            content.sizeDelta = artworkSize;
            content.anchoredPosition *= factor;
            ClampPosition();
        }

        public void Configure(RectTransform artworkViewport, RectTransform artworkContent)
        {
            viewport = artworkViewport;
            content = artworkContent;
            parentCanvas = GetComponentInParent<Canvas>();
            ResetView();
        }

        public void SetArtworkSize(Vector2 size)
        {
            artworkSize = size;
            lastViewportSize = Vector2.zero;
            ResetView();
        }

        public void ZoomIn()
        {
            ZoomBy(ButtonZoomStep);
        }

        public void ZoomOut()
        {
            ZoomBy(-ButtonZoomStep);
        }

        public void CycleZoom()
        {
            if (currentScale >= MaximumScale - .01f) ResetView();
            else ZoomBy(.7f);
        }

        public void ResetView()
        {
            if (content == null)
            {
                return;
            }

            currentScale = MinimumScale;
            content.sizeDelta = artworkSize;
            content.localScale = Vector3.one;
            content.anchoredPosition = Vector2.zero;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            eventData.eligibleForClick=false;
            ClampPosition();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!navigationMode || previousPinchDistance>0 || content == null || currentScale <= MinimumScale)
            {
                return;
            }

            float canvasScale = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
            content.anchoredPosition += eventData.delta / Mathf.Max(canvasScale, 0.01f);
            ClampPosition();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (navigationMode && Mathf.Abs(eventData.scrollDelta.y) > 0.01f)
            {
                ZoomBy(eventData.scrollDelta.y * 0.12f);
            }
        }

        private void ZoomBy(float delta)
        {
            if (content == null)
            {
                return;
            }

            currentScale = Mathf.Clamp(currentScale + delta, MinimumScale, MaximumScale);
            content.localScale = Vector3.one * currentScale;
            ClampPosition();
        }

        private void ClampPosition()
        {
            if (viewport == null || content == null)
            {
                return;
            }

            Vector2 visibleSize = viewport.rect.size;
            Vector2 scaledArtworkSize = artworkSize * currentScale;
            float horizontalLimit = Mathf.Max(0f, (scaledArtworkSize.x - visibleSize.x) * 0.5f);
            float verticalLimit = Mathf.Max(0f, (scaledArtworkSize.y - visibleSize.y) * 0.5f);
            Vector2 position = content.anchoredPosition;
            content.anchoredPosition = new Vector2(
                Mathf.Clamp(position.x, -horizontalLimit, horizontalLimit),
                Mathf.Clamp(position.y, -verticalLimit, verticalLimit));
        }
    }
}
