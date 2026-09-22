using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    /// <summary>Keep live typography in the same coordinate space as the illustrated artboard.</summary>
    [ExecuteAlways]
    public sealed class AirtistScaledLabel : MonoBehaviour
    {
        public RectTransform artboard;
        public float designFontSize = 24;
        [Min(1)] public float designWidth = 1672;
        private TextMeshProUGUI label;
        private void OnEnable() { label = GetComponent<TextMeshProUGUI>(); Apply(); }
        private void LateUpdate() => Apply();
        private void Apply()
        {
            if (!label || !artboard) return;
            float scale=artboard.GetComponent<AirtistFlexibleArtboard>()!=null
                ? AirtistFlexibleArtboard.Scale(artboard,Mathf.Max(1,designWidth))
                : artboard.rect.width/Mathf.Max(1,designWidth);
            float size = designFontSize * scale;
            label.enableAutoSizing = true;
            label.fontSizeMax = size;
            label.fontSizeMin = size * .8f;
        }
    }
}
