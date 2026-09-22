using System;
using UnityEngine;

namespace Airtist.Prototype
{
    [CreateAssetMenu(menuName="AIrtist/Hidden object layout")]
    public sealed class AirtistHiddenObjectCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string chapterId;
            public int artifactIndex;
            public string title;
            public Sprite sprite;
            [Tooltip("Coordinates on the original painting: bottom-left is 0,0.")]
            public Vector2 position;
            [Range(.02f,.3f)] public float widthFraction=.08f;
            public float rotation;
            [Tooltip("Painting-specific light/pigment tint. White preserves the source sprite.")]
            public Color tint=Color.white;
        }
        public Entry[] entries=Array.Empty<Entry>();
        public Entry Find(string chapter,int index) => Array.Find(entries,e=>e!=null && e.chapterId==chapter && e.artifactIndex==index);
    }
}
