using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistStoreScreen : MonoBehaviour
    {
        public AirtistStoreCatalog catalog;
        public AirtistStoreOffer[] offers;
        public GameObject dialog;
        public TMP_Text dialogTitle, dialogBody, dialogPrice;
        public UnityEngine.UI.Button confirm;
        private int selectedProduct = -1;

        private void OnEnable()
        {
            if (offers != null) foreach (var offer in offers) if (offer != null) offer.Refresh(catalog);
            CloseDialog();
        }
        private void OnDisable() => CloseDialog();
        public void OpenProduct(int index)
        {
            if (catalog == null || catalog.products == null || index < 0 || index >= catalog.products.Length || catalog.products[index] == null) return;
            selectedProduct = index;
            var product = catalog.products[index];
            dialogTitle.text = product.title;
            dialogBody.text = product.description + "\n\nДемонстрация интерфейса. Реальные покупки ещё не подключены.";
            dialogPrice.text = "Примерная цена: " + product.previewPrice;
            confirm.gameObject.SetActive(true); dialog.SetActive(true);
        }
        public void ConfirmPreview()
        {
            if (selectedProduct < 0) return;
            // Never grant paid benefits without a verified store transaction.
            dialogBody.text = "Платёжный сервис пока не подключён.\nДеньги не списаны, товары не начислены.";
            dialogPrice.text = "Это прототип магазина";
            confirm.gameObject.SetActive(false);
        }
        public void RestorePurchases()
        {
            selectedProduct = -1;
            dialogTitle.text = "Восстановить покупки";
            dialogBody.text = "Восстановление будет доступно после подключения магазина платформы.\nСейчас сохранённый прогресс не изменён.";
            dialogPrice.text = "Платёжный сервис не подключён";
            confirm.gameObject.SetActive(false); dialog.SetActive(true);
        }
        public void CloseDialog()
        { selectedProduct = -1; if (dialog != null) dialog.SetActive(false); }
    }
}
