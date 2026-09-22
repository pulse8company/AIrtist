using System.Collections.Generic;
using UnityEngine;

namespace Airtist.Prototype
{
    // Callouts remain small while map locations follow pan and zoom.
    public sealed class AirtistMapMarkerLayout : MonoBehaviour
    {
        private sealed class Marker
        {
            public RectTransform Root, Icon, Line, Dot;
            public Vector2 Offset;
        }
        private readonly List<Marker> markers = new List<Marker>();
        private readonly List<Rect> occupied = new List<Rect>();
        private RectTransform viewport, content;
        public void Configure(RectTransform view, RectTransform map) { viewport = view; content = map; }
        public void Register(RectTransform root, RectTransform icon, RectTransform line, RectTransform dot, Vector2 offset)
            => markers.Add(new Marker { Root = root, Icon = icon, Line = line, Dot = dot, Offset = offset });

        private void LateUpdate()
        {
            if (viewport == null || content == null) return;
            occupied.Clear();
            Rect bounds = viewport.rect;
            // Reserve the zoom controls at the bottom right.
            occupied.Add(new Rect(bounds.xMax - 240, bounds.yMin, 240, 150));
            float zoom = Mathf.Max(.01f, content.localScale.x);
            foreach (var marker in markers)
            {
                Vector2 city = viewport.InverseTransformPoint(marker.Root.position);
                bool visible = bounds.Contains(city);
                Vector2 position = city + marker.Offset;
                bool placed = false;
                if (visible)
                {
                    for (int attempt = 0; attempt < 81 && !placed; attempt++)
                    {
                        Vector2 candidate = position;
                        if (attempt > 0)
                        {
                            int ring = (attempt - 1) / 16 + 1;
                            float angle = (attempt - 1) % 16 * Mathf.PI / 8;
                            candidate = city + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (ring * 100);
                        }
                        candidate.x = Mathf.Clamp(candidate.x, bounds.xMin + 70, bounds.xMax - 70);
                        candidate.y = Mathf.Clamp(candidate.y, bounds.yMin + 70, bounds.yMax - 70);
                        Rect box = new Rect(candidate - Vector2.one * 68, Vector2.one * 136);
                        bool overlaps = false;
                        foreach (var other in occupied) if (box.Overlaps(other)) { overlaps = true; break; }
                        if (overlaps) continue;
                        occupied.Add(box); position = candidate; placed = true;
                    }
                }
                marker.Icon.gameObject.SetActive(visible && placed);
                marker.Line.gameObject.SetActive(visible && placed);
                marker.Dot.gameObject.SetActive(visible);
                marker.Dot.localScale = Vector3.one / zoom;
                if (!placed) continue;
                marker.Icon.localScale = Vector3.one / zoom;
                Vector2 offset = marker.Root.InverseTransformPoint(viewport.TransformPoint(position));
                marker.Icon.anchoredPosition = offset;
                marker.Line.anchoredPosition = offset / 2;
                marker.Line.sizeDelta = new Vector2(offset.magnitude, 2 / zoom);
                marker.Line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg);
            }
        }
    }
}
