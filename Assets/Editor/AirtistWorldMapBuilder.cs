using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistWorldMapBuilder
    {
        public const string PrefabPath = "Assets/UI/WorldMap/WorldMapScreen.prefab";
        private const string Original = "Assets/Art/Prototype/WorldMap/WorldMapWooden.png";
        private const string Clean = "Assets/Art/Prototype/WorldMap/WorldMapClean.png";
        private static readonly Rect MapArea = new Rect(22, 106, 1628, 810);
        private static readonly Rect[] Controls = { new Rect(1110, 6, 163, 87), new Rect(1284, 6, 164, 87),
            new Rect(1454, 6, 169, 87), new Rect(1466, 824, 85, 91), new Rect(1560, 824, 86, 91) };

        public static string Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play first.");
            var regions = new[] { new Rect(0, 0, 1672, 941) }.Concat(Controls).ToArray();
            Sprite[] sprites = Slice(Original, regions);
            Sprite mapSprite = Slice(Clean, new[] { MapArea })[0];
            if (!AssetDatabase.IsValidFolder("Assets/UI/WorldMap")) AssetDatabase.CreateFolder("Assets/UI", "WorldMap");
            var root = new GameObject("WorldMapScreen", typeof(RectTransform), typeof(UnityEngine.UI.AspectRatioFitter), typeof(AirtistWorldMapScreen));
            try
            {
                var rect = root.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(1672, 941);
                var fit = root.GetComponent<UnityEngine.UI.AspectRatioFitter>(); fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = 1672f / 941f;
                Image(rect, "WoodenFrame", new Rect(0, 0, 1672, 941), sprites[0], false);
                var viewport = Image(rect, "MapViewport", MapArea, null, false).rectTransform;
                viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
                var map = Image(viewport, "MapContent", new Rect(0, 0, 1672, 941), mapSprite, true).rectTransform;
                map.anchorMin = Vector2.zero; map.anchorMax = Vector2.one; map.offsetMin = map.offsetMax = Vector2.zero;
                var buttons = new UnityEngine.UI.Button[5];
                string[] names = { "Home", "Collection", "Profile", "ZoomOut", "ZoomIn" };
                for (int i = 0; i < buttons.Length; i++)
                {
                    var image = Image(rect, names[i] + "Button", Controls[i], sprites[i + 1], true);
                    buttons[i] = image.gameObject.AddComponent<UnityEngine.UI.Button>(); buttons[i].targetGraphic = image;
                    var colors = buttons[i].colors; colors.disabledColor = Color.white; buttons[i].colors = colors;
                    image.raycastPadding = new Vector4(-3, -23, -3, -23);
                }
                root.GetComponent<AirtistWorldMapScreen>().Configure(viewport, map, buttons);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            var controller = UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            controller.ConfigureWorldMap(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<AirtistWorldMapScreen>());
            EditorUtility.SetDirty(controller); EditorSceneManager.MarkSceneDirty(controller.gameObject.scene); EditorSceneManager.SaveScene(controller.gameObject.scene);
            AssetDatabase.SaveAssets();
            return "Wooden map: native navigation, clamped pan and zoom, no museum markers.";
        }
        private static UnityEngine.UI.Image Image(RectTransform parent, string name, Rect area, Sprite sprite, bool raycast)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(area.xMin / 1672f, 1 - area.yMax / 941f); rect.anchorMax = new Vector2(area.xMax / 1672f, 1 - area.yMin / 941f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<UnityEngine.UI.Image>(); image.sprite = sprite; image.raycastTarget = raycast;
            return image;
        }
        private static Sprite[] Slice(string path, Rect[] regions)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var capability = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (capability == null || !capability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite))
                throw new InvalidOperationException("Map slicing is not supported by importer.");
            var old = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var frames = regions.Select((r, i) => new SpriteRect { name = "Slice" + i, rect = new Rect(r.x, 941 - r.yMax, r.width, r.height),
                pivot = new Vector2(.5f, .5f), alignment = SpriteAlignment.Center,
                spriteID = old.TryGetValue("Slice" + i, out var id) ? id : GUID.Generate() }).ToArray();
            provider.SetSpriteRects(frames);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(frames.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s => s.name);
            return Enumerable.Range(0, regions.Length).Select(i => sprites["Slice" + i]).ToArray();
        }
    }
}
