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
            ClampPosition();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (content == null || currentScale <= MinimumScale)
            {
                return;
            }

            float canvasScale = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
            content.anchoredPosition += eventData.delta / Mathf.Max(canvasScale, 0.01f);
            ClampPosition();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (Mathf.Abs(eventData.scrollDelta.y) > 0.01f)
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
