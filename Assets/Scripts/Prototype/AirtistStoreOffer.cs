using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistStoreOffer : MonoBehaviour
    {
        public int productIndex;
        public UnityEngine.UI.Image icon;
        public TMP_Text title, price;
        public UnityEngine.UI.Button buy;
        public bool showBuyPrefix;
        public void Refresh(AirtistStoreCatalog catalog)
        {
            bool valid = catalog != null && catalog.products != null && productIndex >= 0
                && productIndex < catalog.products.Length && catalog.products[productIndex] != null;
            buy.interactable = valid;
            if (!valid) return;
            var product = catalog.products[productIndex];
            icon.sprite = product.icon;
            title.text = product.title;
            price.text = (showBuyPrefix ? "Купить · " : "") + product.previewPrice;
        }
    }
}
