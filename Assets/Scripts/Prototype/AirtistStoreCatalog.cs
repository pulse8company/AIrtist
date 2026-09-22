using System;
using UnityEngine;

namespace Airtist.Prototype
{
    [CreateAssetMenu(menuName = "AIrtist/Store catalog")]
    public sealed class AirtistStoreCatalog : ScriptableObject
    {
        public enum ProductKind { RemoveAds, Gold, Hints }
        [Serializable]
        public sealed class Product
        {
            public string id;
            public ProductKind kind;
            public string title;
            [TextArea] public string description;
            [Min(0)] public int amount;
            [Tooltip("Mockup only. Production prices must come from the platform store.")]
            public string previewPrice;
            public Sprite icon;
        }
        public Product[] products = Array.Empty<Product>();
    }
}
