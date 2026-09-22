using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistMyGalleryScreen : MonoBehaviour
    {
        public AirtistGalleryCatalog catalog;
        public AirtistGalleryCard cardPrefab;
        public RectTransform content;
        public UnityEngine.UI.ScrollRect scroll;
        public TMP_Text paintingCount, traceCount, museumCount, sectionTitle;
        public UnityEngine.UI.Button[] museumFilters;
        public UnityEngine.UI.Button nextPainting;
        private readonly List<AirtistGalleryCard> cards = new List<AirtistGalleryCard>();
        private Func<int, bool> collected, playable;
        private Func<int, int> found;
        private Action<int> open;
        private int museumFilter = 0;
        private bool wired;
        public Func<int,int> ratingMask;

        public void Bind(Func<int, bool> isCollected, Func<int, bool> isPlayable, Func<int, int> foundCount,
            Action<int> openPainting, Action<int> openMuseum)
        {
            collected = isCollected; playable = isPlayable; found = foundCount; open = openPainting;
            if (!wired)
            {
                foreach (var card in content.GetComponentsInChildren<AirtistGalleryCard>(true)) cards.Add(card);
                for (int i = 0; i < museumFilters.Length; i++)
                {
                    int index = i;
                    museumFilters[i].onClick.AddListener(() => {
                        if (index >= 2) { openMuseum(index - 1); return; }
                        museumFilter = index == 0 ? -1 : 0;
                        Refresh(); scroll.StopMovement(); scroll.verticalNormalizedPosition = 1;
                    });
                }
                wired = true;
            }
            Refresh();
        }

        public void Refresh()
        {
            if (catalog == null || collected == null) return;
            int visible = 0, total = 0, owned = 0, traces = 0, visibleOwned = 0;
            foreach (var painting in catalog.paintings)
            {
                if (painting == null) continue;
                int chapter = painting.chapterIndex;
                bool hasPainting = collected(chapter), canPlay = playable(chapter);
                int count = found(chapter);
                if (hasPainting) owned++;
                traces += count;
                if (museumFilter >= 0 && painting.museum != museumFilter) continue;
                total++; if (hasPainting) visibleOwned++;
                if (visible >= cards.Count) cards.Add(Instantiate(cardPrefab, content));
                var card = cards[visible++]; card.gameObject.SetActive(true);
                card.Present(painting, hasPainting, canPlay, count, () => open(chapter));
                card.PresentRating(ratingMask!=null?ratingMask(chapter):0);
            }
            for (int i = visible; i < cards.Count; i++) cards[i].gameObject.SetActive(false);
            paintingCount.text = owned.ToString(); traceCount.text = traces.ToString(); museumCount.text = "1";
            sectionTitle.text = $"{(museumFilter < 0 ? "Все музеи" : "Лувр")} · собрано {visibleOwned} из {total}";
            for (int i = 0; i < 2; i++)
                museumFilters[i].image.color = (museumFilter < 0 ? i == 0 : i == 1) ? new Color(.80f,.87f,.78f) : new Color(.99f,.96f,.88f);
        }
    }
}
