using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistBuildInfo : ScriptableObject
    {
        public string buildNumber="", builtUtc="", platform="";
        public static string Description
        {
            get
            {
#if UNITY_EDITOR
                var target=UnityEditor.EditorUserBuildSettings.activeBuildTarget;
                string number=target==UnityEditor.BuildTarget.Android?UnityEditor.PlayerSettings.Android.bundleVersionCode.ToString()
                    :target==UnityEditor.BuildTarget.iOS?UnityEditor.PlayerSettings.iOS.buildNumber:"Editor";
                return $"Версия {Application.version} · Сборка {number} · {target}";
#else
                var info=Resources.Load<AirtistBuildInfo>("BuildInfo");
                return $"Версия {Application.version} · Сборка {(info!=null?info.buildNumber:"—")} · {(info!=null?info.platform:Application.platform.ToString())}";
#endif
            }
        }
    }
}
