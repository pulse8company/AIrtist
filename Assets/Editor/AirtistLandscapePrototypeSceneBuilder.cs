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
        private const string PortraitPath = "Assets/Art/Prototype/PortraitOfAmelie.png";
        private const string ScenePath = "Assets/Scenes/AirtistLandscapePrototype.unity";
        private const string FallbackFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        public static string Build()
        {
            AssetDatabase.ImportAsset(BackgroundPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(PortraitPath, ImportAssetOptions.ForceUpdate);

            Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            Sprite portrait = AssetDatabase.LoadAssetAtPath<Sprite>(PortraitPath);
            TMP_FontAsset uiFont = TMP_Settings.defaultFontAsset ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackFontPath);
            if (background == null || portrait == null || uiFont == null)
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
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(root.transform, false);

            AirtistLandscapePrototypeController controller = root.AddComponent<AirtistLandscapePrototypeController>();
            controller.Configure(background, portrait, uiFont);

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
    }
}
