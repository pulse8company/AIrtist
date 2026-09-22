using UnityEngine;

namespace Airtist.Prototype
{
    /// <summary>
    /// Keeps the landscape UI inside the device safe area and chooses a scaling strategy that
    /// preserves all controls on narrow tablets as well as on wide phones and desktop browsers.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class AirtistResponsiveLayout : MonoBehaviour
    {
        private const float DesignAspect = 16f / 9f;

        [SerializeField] private UnityEngine.UI.CanvasScaler canvasScaler;
        [SerializeField] private RectTransform safeAreaRoot;

        private Rect previousSafeArea;
        private int previousWidth;
        private int previousHeight;

        public void Configure(UnityEngine.UI.CanvasScaler scaler, RectTransform safeArea)
        {
            canvasScaler = scaler;
            safeAreaRoot = safeArea;
        }

        private void Awake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Screen.fullScreen = true;
#endif
            ApplyLayout();
        }

        private void OnApplicationFocus(bool focused)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(focused) Screen.fullScreen=true;
#endif
            if(focused) ApplyLayout();
        }

        private void Update()
        {
            if (previousWidth != Screen.width || previousHeight != Screen.height || previousSafeArea != Screen.safeArea)
            {
                ApplyLayout();
            }
        }

        private void ApplyLayout()
        {
            previousWidth = Screen.width;
            previousHeight = Screen.height;
            previousSafeArea = Screen.safeArea;

            if (canvasScaler != null && Screen.height > 0)
            {
                float aspect = Screen.width / (float)Screen.height;
                // Fit width on 4:3-style tablets, fit height on wide landscape phones and web.
                canvasScaler.matchWidthOrHeight = aspect < DesignAspect ? 0f : 1f;
            }

            if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Rect safe = Screen.safeArea;
            safeAreaRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            safeAreaRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
        }
    }
}
