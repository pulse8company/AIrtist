using UnityEngine;

namespace Airtist.Prototype
{
    /// <summary>Normalized UI regions fill their safe viewport instead of letterboxing to 16:9.</summary>
    [DisallowMultipleComponent]
    public sealed class AirtistFlexibleArtboard : MonoBehaviour
    {
        public static void Fill(RectTransform rect)
        {
            var fitter=rect.GetComponent<UnityEngine.UI.AspectRatioFitter>();
            if(fitter!=null) fitter.enabled=false;
            rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            if(rect.GetComponent<AirtistFlexibleArtboard>()==null) rect.gameObject.AddComponent<AirtistFlexibleArtboard>();
        }
        public static float Scale(RectTransform rect,float designWidth=1672)
            => Mathf.Min(rect.rect.width/designWidth,rect.rect.height/941f);
    }
}
