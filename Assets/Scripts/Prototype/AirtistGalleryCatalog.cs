using System;
using UnityEngine;

namespace Airtist.Prototype
{
    [CreateAssetMenu(menuName = "AIrtist/Gallery catalog")]
    public sealed class AirtistGalleryCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Painting
        {
            public string id;
            public string title;
            public string artist;
            [Range(0, 3)] public int museum;
            public Sprite artwork;
            [Tooltip("Index in the playable prototype. -1 means exhibition preview only.")]
            public int chapterIndex = -1;
        }
        public Painting[] paintings = Array.Empty<Painting>();
    }
}
