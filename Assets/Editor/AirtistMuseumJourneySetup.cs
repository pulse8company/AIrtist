using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using TMPro;

namespace Airtist.Prototype.Editor
{
    public static class AirtistMuseumJourneySetup
    {
        public static void Configure(AirtistLandscapePrototypeController controller)
        {
            string[] names = { "Louvre", "Met", "Hermitage", "Prado" };
            var sprites = names.Select(n => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Museums/" + n + ".png")).ToArray();
            if (sprites.Any(s => s == null)) throw new InvalidOperationException("Import the four museum illustrations first.");
            var panel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Museums/JourneyPanel.png");
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AirtistLocalizationBuilder.FontPath);
            if (panel == null || font == null) throw new InvalidOperationException("Journey frame or font missing.");
            controller.ConfigureMuseumJourney(sprites, font, panel);
            var icons = names.Select(n => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Museums/MapIcons/" + n + ".png")).ToArray();
            if (icons.Any(s => s == null)) throw new InvalidOperationException("Import the four map icons first.");
            controller.ConfigureMuseumMapIcons(icons);
            EditorUtility.SetDirty(controller);
        }

        public static string ImportAndConnect()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before importing museum assets.");
            var controller = UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            if (controller == null || controller.gameObject.scene.path != "Assets/Scenes/AirtistLandscapePrototype.unity")
                throw new InvalidOperationException("Open the existing landscape prototype scene.");
            foreach (string name in new[] { "Louvre", "Met", "Hermitage", "Prado", "JourneyPanel", "MapIcons/Louvre", "MapIcons/Met", "MapIcons/Hermitage", "MapIcons/Prado" })
            {
                string path = "Assets/Art/Prototype/Museums/" + name + ".png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("Missing texture " + path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 2048;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                if (name == "JourneyPanel")
                {
                    var factory = new SpriteDataProviderFactories(); factory.Init();
                    var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
                    if (provider == null) throw new InvalidOperationException("Sprite provider unavailable.");
                    provider.InitSpriteEditorDataProvider();
                    var capability = provider.GetDataProvider<ISpriteFrameEditCapability>();
                    if (capability == null || !capability.GetEditCapability().HasCapability(EEditCapability.EditBorder))
                        throw new InvalidOperationException("Panel border editing unsupported.");
                    var frames = provider.GetSpriteRects();
                    if (frames.Length != 1) throw new InvalidOperationException("Expected one panel sprite.");
                    frames[0].border = new Vector4(200, 200, 200, 200);
                    provider.SetSpriteRects(frames); provider.Apply(); importer.SaveAndReimport();
                }
            }
            Configure(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            EditorSceneManager.SaveScene(controller.gameObject.scene);
            AssetDatabase.SaveAssets();
            return "Four museum illustrations and nine-slice journey frame connected. No Play or player build started.";
        }
    }
}
