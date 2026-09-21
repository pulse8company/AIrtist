using System;
using Airtist.Prototype;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Airtist.Prototype.Editor
{
    /// <summary>
    /// Builds the landscape-first playable UI scene through Unity APIs. The original vertical
    /// experiment is intentionally left untouched for comparison while this scene becomes the demo.
    /// </summary>
    public static class AirtistLandscapePrototypeSceneBuilder
    {
        private const string BackgroundPath = "Assets/Art/Prototype/LouvreGalleryBackground.png";
        private const string HomeBackgroundPath = "Assets/Art/Prototype/Home/HomeMenuBackdrop_v1.png";
        private const string PortraitPath = "Assets/Art/Prototype/PortraitOfAmelie.png";
        private const string MonaLisaPath = "Assets/Art/Prototype/Louvre/MonaLisa.jpg";
        private const string LibertyLeadingThePeoplePath = "Assets/Art/Prototype/Louvre/LibertyLeadingThePeople.jpg";
        private const string RaftOfTheMedusaPath = "Assets/Art/Prototype/Louvre/RaftOfTheMedusa.jpg";
        private const string ScenePath = "Assets/Scenes/AirtistLandscapePrototype.unity";
        private const string FallbackFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        public static string Build()
        {
            AssetDatabase.ImportAsset(BackgroundPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(HomeBackgroundPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(PortraitPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(MonaLisaPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(LibertyLeadingThePeoplePath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(RaftOfTheMedusaPath, ImportAssetOptions.ForceUpdate);

            Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            Sprite homeBackground = LoadSprite(HomeBackgroundPath);
            Sprite portrait = AssetDatabase.LoadAssetAtPath<Sprite>(PortraitPath);
            Sprite[] louvrePaintings =
            {
                LoadSprite(MonaLisaPath),
                LoadSprite(LibertyLeadingThePeoplePath),
                LoadSprite(RaftOfTheMedusaPath)
            };
            TMP_FontAsset uiFont = TMP_Settings.defaultFontAsset ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackFontPath);
            if (homeBackground == null || background == null || portrait == null || louvrePaintings[0] == null || louvrePaintings[1] == null || louvrePaintings[2] == null || uiFont == null)
            {
                throw new InvalidOperationException("AIrtist prototype art or TextMeshPro resources are not available.");
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("AIrtistLandscapePrototype", typeof(RectTransform));
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            UnityEngine.UI.CanvasScaler scaler = root.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            root.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            GameObject previewCameraObject = new GameObject("AirtistPreviewCamera", typeof(Camera));
            Camera previewCamera = previewCameraObject.GetComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(1f, 0.956f, 0.875f, 1f);
            previewCamera.orthographic = true;
            previewCamera.depth = -100f;

            GameObject safeAreaObject = new GameObject("SafeArea", typeof(RectTransform));
            safeAreaObject.transform.SetParent(root.transform, false);
            RectTransform safeArea = safeAreaObject.GetComponent<RectTransform>();
            safeArea.anchorMin = Vector2.zero;
            safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = Vector2.zero;
            safeArea.offsetMax = Vector2.zero;

            AirtistResponsiveLayout responsiveLayout = root.AddComponent<AirtistResponsiveLayout>();
            responsiveLayout.Configure(scaler, safeArea);

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(root.transform, false);

            AirtistLandscapePrototypeController controller = safeAreaObject.AddComponent<AirtistLandscapePrototypeController>();
            controller.Configure(homeBackground, background, portrait, louvrePaintings, uiFont);
            EditorUtility.SetDirty(controller);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException("Unity could not save the landscape AIrtist prototype scene.");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            return ScenePath;
        }

        private static Sprite LoadSprite(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.mipmapEnabled))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
    }
}
