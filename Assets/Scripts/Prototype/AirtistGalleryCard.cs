using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistGalleryCard : MonoBehaviour
    {
        public UnityEngine.UI.Image artwork;
        public TMP_Text title, artist, state, actionLabel;
        public UnityEngine.UI.Button action;
        public GameObject locked;
        private Action selected;
        private bool wired;
        public void PresentRating(int mask)
        {
            var row=transform.Find("RatingStars");
            if(row==null) return;
            row.gameObject.SetActive(mask!=0);
            state.gameObject.SetActive(mask==0);
            var stars=row.GetComponentsInChildren<AirtistRatingStar>(true);
            int count=AirtistRestorationRecord.Stars(mask);
            for(int i=0;i<stars.Length;i++) stars[i].color=i<count?new Color(.94f,.66f,.22f):new Color(.77f,.75f,.68f);
        }

        public void Present(AirtistGalleryCatalog.Painting painting, bool collected, bool playable, int found, Action open)
        {
            title.text = painting.title; artist.text = painting.artist;
            artwork.sprite = painting.artwork;
            AirtistPaintingFrame.Attach(artwork);
            artwork.color = playable || collected ? Color.white : new Color(.62f, .59f, .53f);
            state.text = collected ? "В коллекции" : playable ? $"Найдено AI-следов: {found}" : "Следующая картина";
            actionLabel.text = collected ? "Подробнее" : playable ? "Продолжить поиск" : "Закрыто";
            action.interactable = playable || collected;
            action.image.color = collected ? new Color(.80f,.87f,.78f) : playable ? new Color(.91f,.71f,.57f) : new Color(.89f,.87f,.82f);
            locked.SetActive(!playable && !collected);
            selected = open;
            if (!wired) { action.onClick.AddListener(() => selected?.Invoke()); wired = true; }
        }
    }
}
