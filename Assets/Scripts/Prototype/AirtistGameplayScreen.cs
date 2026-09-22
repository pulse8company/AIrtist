using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistGameplayScreen : MonoBehaviour
    {
        public RectTransform viewport, artworkContent;
        public UnityEngine.UI.Image painting;
        public TextMeshProUGUI title, description, feedback, hint, progress;
        public UnityEngine.UI.Button mainTool, markTool, zoomTool, help;
        public UnityEngine.UI.Button[] brushes;
        public TMP_Text[] brushCosts;
        public Color selectedColor = new Color(.35f, .75f, .75f);
        public int SelectedBrush { get; private set; } = -1;

        public System.Action<int> brushRequested;
        public void SelectBrush(int index)
        {
            if (index < 0 || index >= brushes.Length) return;
            SelectedBrush = index;
            brushRequested?.Invoke(index);
        }
    }
}
