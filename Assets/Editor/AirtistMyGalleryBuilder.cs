using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistMyGalleryBuilder
    {
        public const string Folder = "Assets/UI/MyGallery";
        public const string ScreenPath = Folder + "/MyGalleryScreen.prefab";
        public const string CardPath = Folder + "/PaintingCard.prefab";
        public const string CatalogPath = Folder + "/GalleryCatalog.asset";
        private static readonly Color Ink = new Color(.035f, .19f, .23f);
        private static TMP_FontAsset font;
        private static Sprite panel;

        public static string CreateAndConnect()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play first; no gameplay or builds will be started.");
            var controller = UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            if (controller == null) throw new InvalidOperationException("Open AIrtist's landscape scene.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/UI", "MyGallery");
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AirtistLocalizationBuilder.FontPath);
            panel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Museums/JourneyPanel.png");
            if (font == null || panel == null) throw new InvalidOperationException("Missing gallery font or frame.");
            // Subsequent calls preserve all Inspector edits; never regenerate existing assets.
            var catalog = AssetDatabase.LoadAssetAtPath<AirtistGalleryCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<AirtistGalleryCatalog>();
                string[] ids = { "mona-lisa", "liberty", "medusa" };
                string[] titles = { "Мона Лиза", "Свобода, ведущая народ", "Плот «Медузы»" };
                string[] artists = { "Леонардо да Винчи", "Эжен Делакруа", "Теодор Жерико" };
                string[] files = { "MonaLisa", "LibertyLeadingThePeople", "RaftOfTheMedusa" };
                catalog.paintings = new AirtistGalleryCatalog.Painting[3];
                for (int i = 0; i < 3; i++) catalog.paintings[i] = new AirtistGalleryCatalog.Painting {
                    id = ids[i], title = titles[i], artist = artists[i], chapterIndex = i, museum = 0,
                    artwork = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Louvre/" + files[i] + ".jpg") };
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(CardPath) == null) CreateCard();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ScreenPath) == null) CreateScreen(catalog);
            Connect(controller);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            EditorSceneManager.SaveScene(controller.gameObject.scene);
            AssetDatabase.SaveAssets();
            return "Gallery screen, editable painting card and catalog saved and connected; no Play or build.";
        }

        public static void Connect(AirtistLandscapePrototypeController controller)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ScreenPath);
            if (prefab != null) controller.ConfigureMyGallery(prefab.GetComponent<AirtistMyGalleryScreen>());
            var menuPanel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/MyGallery/GalleryPanel.png");
            if (menuPanel != null) controller.ConfigureGalleryMenuPanel(menuPanel);
        }

        public static string AddNextPaintingAction()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AirtistLocalizationBuilder.FontPath);
            panel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/MyGallery/GalleryPanel.png");
            var prefab = PrefabUtility.LoadPrefabContents(ScreenPath);
            try
            {
                var view = prefab.GetComponent<AirtistMyGalleryScreen>();
                if (view.nextPainting == null)
                {
                    var root = (RectTransform)prefab.transform;
                    Place(view.sectionTitle.rectTransform, new Rect(300, 382, 700, 42));
                    view.nextPainting = Button(root, "NextPainting", "Следующая картина", new Rect(1030, 379, 392, 48));
                    var label = view.nextPainting.GetComponentInChildren<TMP_Text>();
                    var scale = label.gameObject.AddComponent<AirtistScaledLabel>();
                    scale.artboard = root; scale.designWidth = 1672; scale.designFontSize = 21;
                    PrefabUtility.SaveAsPrefabAsset(prefab, ScreenPath);
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            AssetDatabase.SaveAssets();
            return "Editable NextPainting button connected above the scrolling cards.";
        }

        public static string ApplyGalleryArt()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play first.");
            foreach (string name in new[] { "GalleryPanel", "GalleryFrame", "GalleryAvatar" }) ImportArt(name);
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AirtistLocalizationBuilder.FontPath);
            panel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/MyGallery/GalleryPanel.png");
            var frame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/MyGallery/GalleryFrame.png");
            var avatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/MyGallery/GalleryAvatar.png");
            foreach (string path in new[] { CardPath, ScreenPath })
            {
                var prefab = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (path == CardPath)
                    {
                        var root = (RectTransform)prefab.transform;
                        var gold = prefab.transform.Find("GoldFrame").GetComponent<UnityEngine.UI.Image>();
                        gold.sprite = frame; gold.type = UnityEngine.UI.Image.Type.Sliced; gold.pixelsPerUnitMultiplier = 6;
                        if (prefab.transform.Find("PaperMat") == null)
                        {
                            var mat = Picture(root, "PaperMat", null, new Rect(19, 21, 342, 438));
                            mat.color = new Color(1, .96f, .86f); mat.transform.SetAsFirstSibling();
                        }
                        if (prefab.transform.Find("CaptionPanel") == null)
                        {
                            var caption = Picture(root, "CaptionPanel", panel, new Rect(12, 308, 356, 160));
                            caption.transform.SetSiblingIndex(3);
                        }
                        gold.transform.SetAsLastSibling();
                    }
                    foreach (var image in prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                    {
                        if (image.sprite != null && AssetDatabase.GetAssetPath(image.sprite).EndsWith("JourneyPanel.png"))
                        { image.sprite = panel; image.type = UnityEngine.UI.Image.Type.Sliced; image.pixelsPerUnitMultiplier = 6; }
                    }
                    if (path == ScreenPath) prefab.transform.Find("ProfilePanel/Avatar").GetComponent<UnityEngine.UI.Image>().sprite = avatar;
                    foreach (var label in prefab.GetComponentsInChildren<TextMeshProUGUI>(true))
                    {
                        var card = label.GetComponentInParent<AirtistGalleryCard>();
                        var scale = label.GetComponent<AirtistScaledLabel>() ?? label.gameObject.AddComponent<AirtistScaledLabel>();
                        scale.artboard = card != null ? (RectTransform)card.transform : (RectTransform)prefab.transform;
                        scale.designWidth = card != null ? 380 : 1672;
                        scale.designFontSize = label.fontSizeMax;
                    }
                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(prefab); }
            }
            AssetDatabase.SaveAssets();
            return "Separate gold frame, parchment panel and avatar applied; all captions remain editable UI.";
        }

        private static void ImportArt(string name)
        {
            string path = "Assets/Art/Prototype/MyGallery/" + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (edit == null || !edit.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite)
                || !edit.GetEditCapability().HasCapability(EEditCapability.EditBorder))
                throw new InvalidOperationException("Sprite border/rect creation unsupported.");
            var texture = new Texture2D(2, 2);
            Rect bounds;
            try
            {
                ImageConversion.LoadImage(texture, System.IO.File.ReadAllBytes(path));
                var pixels = texture.GetPixels32(); int left = texture.width, right = 0, bottom = texture.height, top = 0;
                for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                    if (pixels[y * texture.width + x].a > 8) { left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y); }
                bounds = new Rect(left, bottom, right - left + 1, top - bottom + 1);
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
            var previous = provider.GetSpriteRects();
            var id = previous.Length == 1 ? previous[0].spriteID : GUID.Generate();
            var border = name == "GalleryAvatar" ? Vector4.zero : name == "GalleryFrame" ? new Vector4(180,180,180,180) : new Vector4(160,100,160,100);
            provider.SetSpriteRects(new[] { new SpriteRect { name = name, spriteID = id, rect = bounds,
                pivot = Vector2.one * .5f, alignment = SpriteAlignment.Center, border = border } });
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(name, id) });
            provider.Apply(); importer.SaveAndReimport();
        }

        private static void CreateCard()
        {
            var root = Rect(null, "PaintingCard", new Vector2(380, 480));
            try
            {
                var card = root.gameObject.AddComponent<AirtistGalleryCard>();
                Picture(root, "GoldFrame", panel, new Rect(0, 0, 380, 480));
                card.artwork = Picture(root, "Painting", null, new Rect(19, 22, 342, 286), true);
                card.title = Text(root, "Title", "Название картины", new Rect(22, 316, 336, 48), 25);
                card.artist = Text(root, "Artist", "Автор", new Rect(22, 366, 336, 28), 18);
                card.state = Text(root, "State", "В коллекции", new Rect(22, 395, 336, 26), 17);
                card.action = Button(root, "OpenPainting", "Подробнее", new Rect(35, 430, 310, 38));
                card.actionLabel = card.action.GetComponentInChildren<TMP_Text>();
                var locked = Picture(root, "LockedBadge", panel, new Rect(147, 125, 86, 56));
                Text(locked.rectTransform, "LockCaption", "Закрыто", new Rect(5, 12, 76, 30), 15);
                card.locked = locked.gameObject;
                PrefabUtility.SaveAsPrefabAsset(root.gameObject, CardPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
        }

        private static void CreateScreen(AirtistGalleryCatalog catalog)
        {
            var root = Rect(null, "MyGalleryScreen", new Vector2(1672, 941));
            try
            {
                var view = root.gameObject.AddComponent<AirtistMyGalleryScreen>();
                var fit = root.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = 1672f / 941;
                var backdrop = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Home/HomeMenuBackdrop_v2.png");
                Picture(root, "ParisStudioBackground", backdrop, new Rect(0, 0, 1672, 941));
                Text(root, "ScreenTitle", "Моя галерея", new Rect(390, 111, 892, 58), 49);
                var profile = Picture(root, "ProfilePanel", panel, new Rect(210, 174, 1242, 138)).rectTransform;
                var portrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Home/AmelieCharacter_v1.png");
                Picture(profile, "Avatar", portrait, new Rect(28, 12, 120, 112), true);
                Text(profile, "PlayerName", "Амели", new Rect(165, 22, 310, 43), 31, false);
                Text(profile, "PlayerRank", "Начинающий коллекционер", new Rect(165, 72, 370, 37), 22, false);
                view.paintingCount = Text(profile, "PaintingCount", "0", new Rect(570, 23, 170, 44), 34);
                Text(profile, "PaintingCaption", "Картин", new Rect(570, 76, 170, 32), 21);
                view.traceCount = Text(profile, "TraceCount", "0", new Rect(795, 23, 170, 44), 34);
                Text(profile, "TraceCaption", "AI-следов", new Rect(795, 76, 170, 32), 21);
                view.museumCount = Text(profile, "MuseumCount", "1", new Rect(1020, 23, 170, 44), 34);
                Text(profile, "MuseumCaption", "Музеев", new Rect(1020, 76, 170, 32), 21);
                var filters = Rect(root, "MuseumFilters", new Vector2(1172, 52));
                Place(filters, new Rect(250, 323, 1172, 52));
                view.museumFilters = new UnityEngine.UI.Button[5];
                string[] museums = { "Все музеи", "Лувр", "Метрополитен", "Эрмитаж", "Прадо" };
                for (int i = 0; i < 5; i++)
                {
                    view.museumFilters[i] = Button(filters, "Filter_" + i, museums[i], new Rect(i * 236, 0, 228, 52));
                    if (i > 1) Text(view.museumFilters[i].transform as RectTransform, "Availability", "скоро", new Rect(76, 35, 80, 15), 11);
                }
                view.sectionTitle = Text(root, "MuseumProgress", "Лувр · собрано 0 из 3", new Rect(300, 382, 1072, 42), 28);
                var scrollRoot = Rect(root, "PaintingScrollView", new Vector2(1172, 475));
                Place(scrollRoot, new Rect(250, 430, 1172, 475));
                view.scroll = scrollRoot.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
                var viewport = Rect(scrollRoot, "Viewport", scrollRoot.sizeDelta); Stretch(viewport);
                var hit = viewport.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.white;
                viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
                var content = Rect(viewport, "PaintingGrid", new Vector2(1172, 0));
                content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f, 1);
                content.sizeDelta = Vector2.zero; content.anchoredPosition = Vector2.zero;
                var grid = content.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3;
                grid.spacing = new Vector2(22, 26); grid.padding = new RectOffset(4, 4, 4, 18);
                grid.childAlignment = TextAnchor.UpperLeft;
                content.gameObject.AddComponent<AirtistGalleryGrid>().cardHeightToWidth = 480f / 380;
                var size = content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                size.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                size.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                view.scroll.viewport = viewport; view.scroll.content = content;
                view.scroll.horizontal = false; view.scroll.vertical = true;
                view.scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
                view.scroll.scrollSensitivity = 38; view.scroll.inertia = true; view.scroll.decelerationRate = .135f;
                view.catalog = catalog; view.content = content;
                view.cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPath).GetComponent<AirtistGalleryCard>();
                for (int i = 0; i < catalog.paintings.Length; i++)
                {
                    var card = ((GameObject)PrefabUtility.InstantiatePrefab(view.cardPrefab.gameObject, content)).GetComponent<AirtistGalleryCard>();
                    card.name = "Painting_" + catalog.paintings[i].id;
                    card.title.text = catalog.paintings[i].title; card.artist.text = catalog.paintings[i].artist;
                    card.artwork.sprite = catalog.paintings[i].artwork;
                    card.state.text = "Не найдена"; card.locked.SetActive(i != 0);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(card.title);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(card.artist);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(card.artwork);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(card.state);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(card.locked);
                }
                content.GetComponent<AirtistGalleryGrid>().Refresh();
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                PrefabUtility.SaveAsPrefabAsset(root.gameObject, ScreenPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.sizeDelta = size; return rect;
        }
        private static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static void Place(RectTransform rect, Rect area)
        {
            var parent = (RectTransform)rect.parent;
            Vector2 size = parent.rect.size;
            rect.anchorMin = new Vector2(area.xMin / size.x, 1 - area.yMax / size.y);
            rect.anchorMax = new Vector2(area.xMax / size.x, 1 - area.yMin / size.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static UnityEngine.UI.Image Picture(RectTransform parent, string name, Sprite sprite, Rect area, bool preserve = false)
        {
            var rect = Rect(parent, name, area.size); Place(rect, area);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.sprite = sprite;
            image.raycastTarget = false; image.preserveAspect = preserve;
            if (sprite == panel) { image.type = UnityEngine.UI.Image.Type.Sliced; image.pixelsPerUnitMultiplier = 6; }
            return image;
        }
        private static TMP_Text Text(RectTransform parent, string name, string value, Rect area, float fontSize, bool center = true)
        {
            var rect = Rect(parent, name, area.size); Place(rect, area);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font = font; text.text = value;
            text.color = Ink; text.fontSize = fontSize; text.enableAutoSizing = true;
            text.fontSizeMax = fontSize; text.fontSizeMin = fontSize * .75f;
            text.alignment = center ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false; return text;
        }
        private static UnityEngine.UI.Button Button(RectTransform parent, string name, string value, Rect area)
        {
            var image = Picture(parent, name, panel, area); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
            Text(image.rectTransform, "Caption", value, new Rect(12, 3, area.width - 24, area.height - 6), 21);
            return button;
        }
    }
}
