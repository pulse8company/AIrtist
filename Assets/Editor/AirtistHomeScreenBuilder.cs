using System;
using System.Collections.Generic;
using System.Linq;
using Airtist.Prototype;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    /// <summary>
    /// Slices the approved artwork into disjoint image regions and real selectable buttons.
    /// No full-screen picture remains behind the controls, so their visual states are independent.
    /// Coordinates are measured in the original 1672 x 941 design, from its top-left corner.
    /// </summary>
    public static class AirtistHomeScreenBuilder
    {
        public const string ArtPath = "Assets/Art/Prototype/Home/HomeApprovedReference.png";
        public const string PrefabPath = "Assets/UI/Home/HomeScreen.prefab";
        private const float Width = 1672f;
        private const float Height = 941f;

        private static readonly string[] Names =
        {
            "GiftButton", "MapButton", "CollectionButton", "StoreButton",
            "ProfileButton", "ContinueButton", "DailyRewardButton", "WorldMapButton"
        };

        private static readonly Rect[] ControlRects =
        {
            new Rect(875, 6, 132, 84), new Rect(1017, 7, 117, 87),
            new Rect(1144, 7, 139, 87), new Rect(1294, 7, 138, 87),
            new Rect(1443, 7, 139, 87), new Rect(108, 723, 438, 96),
            new Rect(559, 703, 400, 117), new Rect(1047, 721, 472, 111)
        };

        public static string Build()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Stop Play Mode before importing Home.");

            AssetDatabase.ImportAsset(ArtPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(ArtPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Approved Home image is missing.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.sRGBTexture = true;
            importer.SaveAndReimport();

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new InvalidOperationException("No sprite data provider.");
            provider.InitSpriteEditorDataProvider();
            var capability = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (capability == null || !capability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite))
                throw new InvalidOperationException("Importer does not permit sprite slicing; aborted.");

            List<(string name, Rect rect)> regions = PartitionArtwork();
            for (int i = 0; i < Names.Length; i++) regions.Add((Names[i], ControlRects[i]));
            var previous = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var frames = regions.Select(r => new SpriteRect
            {
                name = r.name,
                rect = new Rect(r.rect.x, Height - r.rect.yMax, r.rect.width, r.rect.height),
                pivot = new Vector2(0.5f, 0.5f),
                alignment = SpriteAlignment.Center,
                spriteID = previous.TryGetValue(r.name, out var id) ? id : GUID.Generate()
            }).ToArray();
            provider.SetSpriteRects(frames);
            var ids = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (ids != null)
                ids.SetNameFileIdPairs(frames.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(ArtPath).OfType<Sprite>().ToDictionary(s => s.name);
            if (sprites.Count != regions.Count) throw new InvalidOperationException("Incomplete Home sprite import.");

            EnsureFolder("Assets", "UI");
            EnsureFolder("Assets/UI", "Home");
            var root = new GameObject("HomeScreen", typeof(RectTransform), typeof(UnityEngine.UI.AspectRatioFitter), typeof(AirtistHomeScreen));
            try
            {
                var rootRect = root.GetComponent<RectTransform>();
                rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
                rootRect.sizeDelta = new Vector2(Width, Height);
                var fitter = root.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                fitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
                fitter.aspectRatio = Width / Height;

                foreach (var region in regions.Where(r => !r.name.EndsWith("Button")))
                    MakeImage(rootRect, region.name, region.rect, sprites[region.name], false);

                var controls = new UnityEngine.UI.Button[Names.Length];
                for (int i = 0; i < controls.Length; i++)
                {
                    var graphic = MakeImage(rootRect, Names[i], ControlRects[i], sprites[Names[i]], true);
                    // More vertical hit room helps landscape phones; avoid overlapping neighbours.
                    graphic.raycastPadding = new Vector4(-2f, -9f, -2f, -9f);
                    var button = graphic.gameObject.AddComponent<UnityEngine.UI.Button>();
                    button.targetGraphic = graphic;
                    var colors = button.colors;
                    colors.normalColor = Color.white;
                    colors.highlightedColor = new Color(1.06f, 1.04f, 1f, 1f);
                    colors.pressedColor = new Color(0.79f, 0.75f, 0.67f, 1f);
                    colors.selectedColor = Color.white;
                    colors.disabledColor = new Color(0.65f, 0.65f, 0.65f, 1f);
                    colors.fadeDuration = 0.09f;
                    button.colors = colors;
                    controls[i] = button;
                }

                var badge = MakeImage(rootRect, "DailyRewardClaimed", new Rect(642, 747, 282, 48), null, false);
                badge.color = new Color(0.97f, 0.84f, 0.51f, 1f);
                var labelObject = new GameObject("ClaimedLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(badge.transform, false);
                var labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
                var label = labelObject.GetComponent<TextMeshProUGUI>();
                label.font = TMP_Settings.defaultFontAsset;
                label.text = "Бонус получен";
                label.fontSize = 27f;
                label.enableAutoSizing = true;
                label.fontSizeMin = 12f;
                label.fontSizeMax = 27f;
                label.color = new Color(0.18f, 0.12f, 0.05f, 1f);
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                badge.gameObject.SetActive(false);
                root.GetComponent<AirtistHomeScreen>().Configure(controls, badge.gameObject);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }

            AssetDatabase.SaveAssets();
            ApplyToOpenScene();
            return $"{PrefabPath}: {sprites.Count} sprites, {Names.Length} buttons";
        }

        public static void ApplyToOpenScene()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            if (controller == null) throw new InvalidOperationException("Open the AIrtist prototype scene first.");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<AirtistHomeScreen>();
            Undo.RecordObject(controller, "Apply approved Home artwork");
            controller.ConfigureHomeScreen(prefab);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            EditorSceneManager.SaveScene(controller.gameObject.scene);
        }

        private static List<(string name, Rect rect)> PartitionArtwork()
        {
            // Cut only around controls. Static art is reconstructed exactly, with no duplicate controls behind it.
            var cuts = new SortedSet<float> { 0f, Height };
            foreach (Rect r in ControlRects) { cuts.Add(r.yMin); cuts.Add(r.yMax); }
            float[] ys = cuts.ToArray();
            var regions = new List<(string, Rect)>();
            for (int i = 0; i < ys.Length - 1; i++)
            {
                float top = ys[i], bottom = ys[i + 1], cursor = 0f;
                foreach (Rect control in ControlRects.Where(r => r.yMin < bottom && r.yMax > top).OrderBy(r => r.x))
                {
                    if (control.xMin > cursor)
                        regions.Add(($"Artwork_{regions.Count:00}", new Rect(cursor, top, control.xMin - cursor, bottom - top)));
                    cursor = control.xMax;
                }
                if (cursor < Width)
                    regions.Add(($"Artwork_{regions.Count:00}", new Rect(cursor, top, Width - cursor, bottom - top)));
            }
            float area = regions.Sum(r => r.Item2.width * r.Item2.height) + ControlRects.Sum(r => r.width * r.height);
            if (Mathf.Abs(area - Width * Height) > 0.5f) throw new InvalidOperationException("Home slices do not cover the reference exactly.");
            return regions;
        }

        private static UnityEngine.UI.Image MakeImage(RectTransform parent, string name, Rect sourceRect, Sprite sprite, bool interactive)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(sourceRect.xMin / Width, 1f - sourceRect.yMax / Height);
            rect.anchorMax = new Vector2(sourceRect.xMax / Width, 1f - sourceRect.yMin / Height);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.type = UnityEngine.UI.Image.Type.Simple;
            image.raycastTarget = interactive;
            return image;
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
