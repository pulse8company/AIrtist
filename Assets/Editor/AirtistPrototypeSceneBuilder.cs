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
    /// Creates the first playable AIrtist scene through Unity APIs, preserving the template scene.
    /// </summary>
    public static class AirtistPrototypeSceneBuilder
    {
        private const string BackgroundPath = "Assets/Art/Prototype/LouvreGalleryBackground.png";
        private const string PortraitPath = "Assets/Art/Prototype/PortraitOfAmelie.png";
        private const string ScenePath = "Assets/Scenes/AirtistPrototype.unity";

        public static string Build()
        {
            AssetDatabase.ImportAsset(BackgroundPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(PortraitPath, ImportAssetOptions.ForceUpdate);

            var background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            var portrait = AssetDatabase.LoadAssetAtPath<Sprite>(PortraitPath);
            var font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            }

            if (background == null || portrait == null || font == null)
            {
                throw new System.InvalidOperationException("AIrtist prototype assets or TextMeshPro resources are not available.");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("AIrtistPrototype", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = root.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(root.transform, false);

            var controller = root.AddComponent<AirtistPrototypeController>();
            controller.Configure(background, portrait, font);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new System.InvalidOperationException("Unity could not save the AIrtist prototype scene.");
            }

            AssetDatabase.SaveAssets();
            return ScenePath;
        }
    }
}
