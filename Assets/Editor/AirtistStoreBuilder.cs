using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistStoreBuilder
    {
        public const string Folder = "Assets/UI/Store";
        public const string ScreenPath = Folder + "/StoreScreen.prefab";
        public const string CatalogPath = Folder + "/StoreCatalog.asset";
        private const string ArtPath = "Assets/Art/Prototype/Store/StoreApprovedSketch.png";
        private static Sprite panel;
        private static TMP_FontAsset font;
        private static readonly Color Ink = new Color(.035f, .19f, .23f);
        private static RectTransform artboard;

        public static string CreateAndConnect()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before applying the store.");
            var controller = UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            if (controller == null || controller.gameObject.scene.path != "Assets/Scenes/AirtistLandscapePrototype.unity")
                throw new InvalidOperationException("Open the AIrtist landscape scene.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/UI", "Store");
            panel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/MyGallery/GalleryPanel.png");
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AirtistLocalizationBuilder.FontPath);
            if (panel == null || font == null) throw new InvalidOperationException("Store panel/font missing.");
            var icons = ImportIcons();
            var catalog = AssetDatabase.LoadAssetAtPath<AirtistStoreCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<AirtistStoreCatalog>();
                string[] ids = { "remove_ads", "gold_500", "gold_1200", "gold_3000", "hints_3", "hints_10", "hints_25" };
                string[] names = { "Без рекламы", "500 золота", "1 200 золота", "3 000 золота", "3 подсказки", "10 подсказок", "25 подсказок" };
                string[] prices = { "499 ₽", "99 ₽", "199 ₽", "449 ₽", "99 ₽", "249 ₽", "499 ₽" };
                int[] amounts = { 0, 500, 1200, 3000, 3, 10, 25 };
                catalog.products = new AirtistStoreCatalog.Product[7];
                for (int i = 0; i < 7; i++) catalog.products[i] = new AirtistStoreCatalog.Product {
                    id = ids[i], title = names[i], previewPrice = prices[i], amount = amounts[i], icon = icons[i],
                    kind = i == 0 ? AirtistStoreCatalog.ProductKind.RemoveAds : i < 4 ? AirtistStoreCatalog.ProductKind.Gold : AirtistStoreCatalog.ProductKind.Hints,
                    description = i == 0 ? "Отключает обязательную рекламу навсегда. Разовая покупка. Видео за награду остаётся добровольным." : i < 4 ? "Набор игровой валюты: " + names[i] + "." : "Подсказки для поиска AI-дорисовок: " + amounts[i] + "." };
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ScreenPath) == null) CreateScreen(catalog);
            Connect(controller); EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            EditorSceneManager.SaveScene(controller.gameObject.scene); AssetDatabase.SaveAssets();
            return "Store prefab and 7-product catalog saved. Native buttons wired. No purchases, Play or builds started.";
        }

        public static void Connect(AirtistLandscapePrototypeController controller)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ScreenPath);
            if (prefab != null) controller.ConfigureStore(prefab.GetComponent<AirtistStoreScreen>());
        }

        private static Sprite[] ImportIcons()
        {
            AssetDatabase.ImportAsset(ArtPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(ArtPath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var capability = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (capability == null || !capability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite))
                throw new InvalidOperationException("Store sprite slicing unsupported.");
            var old = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            // Only illustrations are reused. Prices, titles and buttons remain native editable UI.
            Rect[] areas = { new Rect(230,263,326,220), new Rect(650,310,118,87), new Rect(649,448,122,104),
                new Rect(650,596,122,129), new Rect(1090,294,120,108), new Rect(1090,441,123,115), new Rect(1090,593,124,129) };
            var frames = areas.Select((r,i) => new SpriteRect { name = "ProductIcon" + i,
                spriteID = old.TryGetValue("ProductIcon" + i, out var id) ? id : GUID.Generate(),
                rect = new Rect(r.x, 941-r.yMax, r.width, r.height), pivot = Vector2.one * .5f, alignment = SpriteAlignment.Center }).ToArray();
            provider.SetSpriteRects(frames);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(frames.Select(r => new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var all = AssetDatabase.LoadAllAssetsAtPath(ArtPath).OfType<Sprite>().ToDictionary(s => s.name);
            return Enumerable.Range(0,7).Select(i => all["ProductIcon"+i]).ToArray();
        }

        private static void CreateScreen(AirtistStoreCatalog catalog)
        {
            artboard = NewRect(null, "StoreScreen", new Vector2(1672,941));
            try
            {
                var view = artboard.gameObject.AddComponent<AirtistStoreScreen>(); view.catalog = catalog;
                var fitter = artboard.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                fitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio = 1672f/941;
                Image(artboard,"ParisStudioBackground",AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Home/HomeMenuBackdrop_v2.png"),new Rect(0,0,1672,941));
                Label(artboard,"StoreTitle","Магазин",new Rect(470,112,732,62),51);
                var noAds = Image(artboard,"NoAdsPanel",panel,new Rect(170,190,440,626)).rectTransform;
                var gold = Image(artboard,"GoldPanel",panel,new Rect(624,190,425,606)).rectTransform;
                var hints = Image(artboard,"HintsPanel",panel,new Rect(1064,190,438,606)).rectTransform;
                Label(gold,"SectionTitle","Золото",new Rect(20,15,385,54),37);
                Label(hints,"SectionTitle","Подсказки",new Rect(20,15,398,54),37);
                view.offers = new AirtistStoreOffer[7];
                var featured = noAds.gameObject.AddComponent<AirtistStoreOffer>(); featured.productIndex=0; featured.showBuyPrefix=true;
                featured.title=Label(noAds,"ProductTitle",catalog.products[0].title,new Rect(24,15,392,55),37);
                featured.icon=Image(noAds,"NoAdsIcon",catalog.products[0].icon,new Rect(57,78,326,220),true);
                Label(noAds,"Benefit","Искусство без пауз",new Rect(24,301,392,48),29);
                Label(noAds,"Description","Отключает обязательную\nрекламу",new Rect(30,351,380,63),24);
                Label(noAds,"PurchaseType","Навсегда · разовая покупка",new Rect(35,420,370,44),21);
                featured.buy=Button(noAds,"BuyNoAds","Купить · 499 ₽",new Rect(38,474,364,76),true);
                featured.price=featured.buy.GetComponentInChildren<TMP_Text>();
                UnityEventTools.AddIntPersistentListener(featured.buy.onClick,view.OpenProduct,0);
                Label(noAds,"RewardedNote","Видео за награду — по желанию",new Rect(30,558,380,36),18);
                view.offers[0]=featured;
                for(int i=1;i<7;i++)
                {
                    var parent=i<4?gold:hints; int row=i<4?i-1:i-4;
                    var item=Image(parent,"Offer_"+catalog.products[i].id,panel,new Rect(15,91+row*155,parent.rect.width-30,141)).rectTransform;
                    var offer=item.gameObject.AddComponent<AirtistStoreOffer>(); offer.productIndex=i;
                    offer.icon=Image(item,"ProductIcon",catalog.products[i].icon,new Rect(12,12,120,116),true);
                    offer.title=Label(item,"ProductTitle",catalog.products[i].title,new Rect(143,23,112,95),25);
                    offer.buy=Button(item,"Buy","",new Rect(item.rect.width-127,37,112,68),false);
                    offer.price=offer.buy.GetComponentInChildren<TMP_Text>(); offer.price.text=catalog.products[i].previewPrice;
                    UnityEventTools.AddIntPersistentListener(offer.buy.onClick,view.OpenProduct,i); view.offers[i]=offer;
                }
                var restore=Button(artboard,"RestorePurchases","Восстановить покупки",new Rect(610,820,450,54),false);
                UnityEventTools.AddPersistentListener(restore.onClick,view.RestorePurchases);
                Label(artboard,"DemoPriceNotice","Цены примерные — реальные покупки пока не подключены",new Rect(360,884,952,32),20);
                CreateDialog(view);
                PrefabUtility.SaveAsPrefabAsset(artboard.gameObject,ScreenPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(artboard.gameObject); artboard=null; }
        }

        private static void CreateDialog(AirtistStoreScreen view)
        {
            var shade=Image(artboard,"PurchaseDialog",null,new Rect(0,110,1672,831));
            shade.color=new Color(.02f,.08f,.10f,.75f); shade.raycastTarget=true;
            view.dialog=shade.gameObject;
            var card=Image(shade.rectTransform,"DialogPanel",panel,new Rect(366,100,940,580)).rectTransform;
            view.dialogTitle=Label(card,"ProductTitle","Товар",new Rect(55,37,830,65),38);
            view.dialogBody=Label(card,"ProductDescription","Описание",new Rect(70,123,800,230),25);
            view.dialogPrice=Label(card,"Price","Цена",new Rect(70,371,800,45),25);
            view.confirm=Button(card,"ConfirmPreview","Продолжить · демо",new Rect(86,452,370,74),true);
            UnityEventTools.AddPersistentListener(view.confirm.onClick,view.ConfirmPreview);
            var close=Button(card,"CloseDialog","Закрыть",new Rect(484,452,370,74),false);
            UnityEventTools.AddPersistentListener(close.onClick,view.CloseDialog);
            view.dialog.SetActive(false);
        }
        private static RectTransform NewRect(Transform parent,string name,Vector2 size)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.sizeDelta=size;return r;}
        private static void Place(RectTransform r,Rect a)
        {var s=((RectTransform)r.parent).rect.size;r.anchorMin=new Vector2(a.xMin/s.x,1-a.yMax/s.y);r.anchorMax=new Vector2(a.xMax/s.x,1-a.yMin/s.y);r.offsetMin=r.offsetMax=Vector2.zero;}
        private static UnityEngine.UI.Image Image(RectTransform parent,string name,Sprite sprite,Rect area,bool preserve=false)
        {var r=NewRect(parent,name,area.size);Place(r,area);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.preserveAspect=preserve;image.raycastTarget=false;if(sprite==panel){image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=6;}return image;}
        private static TMP_Text Label(RectTransform parent,string name,string text,Rect area,float size)
        {var r=NewRect(parent,name,area.size);Place(r,area);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.text=text;t.font=font;t.color=Ink;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;var scale=r.gameObject.AddComponent<AirtistScaledLabel>();scale.artboard=artboard;scale.designFontSize=size;return t;}
        private static UnityEngine.UI.Button Button(RectTransform parent,string name,string text,Rect area,bool coral)
        {var image=Image(parent,name,panel,area);image.raycastTarget=true;image.color=coral?new Color(1,.55f,.40f):new Color(.24f,.65f,.65f);var b=image.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;var t=Label(image.rectTransform,"Caption",text,new Rect(10,5,area.width-20,area.height-10),25);t.color=new Color(1,.97f,.86f);return b;}
    }
}
