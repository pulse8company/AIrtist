using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    // Stamp the actual player build, never a manually maintained UI version string.
    public sealed class AirtistBuildInfoWriter : IPreprocessBuildWithReport
    {
        public int callbackOrder=>-100;
        public void OnPreprocessBuild(BuildReport report)
        {
            const string path="Assets/Resources/BuildInfo.asset";
            var info=AssetDatabase.LoadAssetAtPath<AirtistBuildInfo>(path);
            if(info==null){info=ScriptableObject.CreateInstance<AirtistBuildInfo>();AssetDatabase.CreateAsset(info,path);}
            info.platform=report.summary.platform.ToString();
            info.buildNumber=report.summary.platform==BuildTarget.Android?PlayerSettings.Android.bundleVersionCode.ToString()
                :report.summary.platform==BuildTarget.iOS?PlayerSettings.iOS.buildNumber:PlayerSettings.macOS.buildNumber;
            info.builtUtc=DateTime.UtcNow.ToString("O");
            EditorUtility.SetDirty(info);AssetDatabase.SaveAssetIfDirty(info);
        }
    }
}
