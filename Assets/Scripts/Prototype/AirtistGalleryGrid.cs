using UnityEngine;

namespace Airtist.Prototype
{
    [ExecuteAlways]
    [RequireComponent(typeof(UnityEngine.UI.GridLayoutGroup))]
    public sealed class AirtistGalleryGrid : MonoBehaviour
    {
        [Min(.5f)] public float cardHeightToWidth = 1.28f;
        private UnityEngine.UI.GridLayoutGroup grid;
        private void OnEnable() => Refresh();
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            if (grid == null) grid = GetComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            float width = Mathf.Max(1, (((RectTransform)transform).rect.width - grid.padding.horizontal - grid.spacing.x * 2) / 3);
            var size = new Vector2(width, width * cardHeightToWidth);
            if ((grid.cellSize - size).sqrMagnitude > .01f) grid.cellSize = size;
        }
    }
}
