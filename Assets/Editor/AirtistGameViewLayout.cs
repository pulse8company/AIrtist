using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airtist.Prototype.Editor
{
    /// <summary>Editor-only preview framing. Never starts Play or changes player resolution.</summary>
    [InitializeOnLoad]
    public static class AirtistGameViewLayout
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        static AirtistGameViewLayout()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.delayCall += FitExistingViews;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredPlayMode)
                EditorApplication.delayCall += FitExistingViews;
        }

        [MenuItem("AIrtist/Preview/Fit Game View")]
        public static void FitExistingViews()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (SceneManager.GetActiveScene().path != "Assets/Scenes/AirtistLandscapePrototype.unity") return;
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            if (type == null) return;
            foreach (var instance in Resources.FindObjectsOfTypeAll(type))
            {
                var window = (EditorWindow)instance;
                // Do not create/focus windows or change the selected device/aspect ratio.
                window.maximized = false;
                var behavior = type.GetProperty("enterPlayModeBehavior", Flags);
                if (behavior != null && behavior.CanWrite && behavior.PropertyType.IsEnum &&
                    Enum.IsDefined(behavior.PropertyType, "PlayFocused"))
                    behavior.SetValue(window, Enum.Parse(behavior.PropertyType, "PlayFocused"));
                var minimum = type.GetProperty("minScale", Flags);
                var snap = type.GetMethod("SnapZoom", Flags, null, new[] { typeof(float) }, null);
                if (minimum != null && snap != null)
                    snap.Invoke(window, new object[] { (float)minimum.GetValue(window) });
                window.Repaint();
            }
        }
    }
}
