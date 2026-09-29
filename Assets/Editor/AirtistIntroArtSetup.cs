using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistIntroArtSetup
    {
        public static string Import()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first.");
            const string source="ArtReview/NextStage-2026-09-26/";
            const string folder="Assets/Art/Introduction/";
            foreach(string name in new[]{"Characters-proposal.png","Intro-storyboards-proposal.png"})
                if(!File.Exists(source+name))throw new FileNotFoundException(source+name);
            Directory.CreateDirectory(folder);
            Texture2D ImportImage(string sourceName,string targetName)
            {
                string path=folder+targetName+".png";
                if(!File.Exists(path))File.Copy(source+sourceName,path);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;
                importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;
                importer.textureCompression=TextureImporterCompression.Compressed;
                var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;
                android.maxTextureSize=2048;android.format=TextureImporterFormat.ASTC_6x6;
                importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            var portraits=ImportImage("Characters-proposal.png","CharacterPortraits");
            var comic=ImportImage("Intro-storyboards-proposal.png","StoryPanels");
            const string config="Assets/Resources/IntroductionArt.asset";
            var art=AssetDatabase.LoadAssetAtPath<AirtistIntroArt>(config);
            if(art==null){art=ScriptableObject.CreateInstance<AirtistIntroArt>();AssetDatabase.CreateAsset(art,config);}
            art.portraits=portraits;art.comic=comic;EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            return "Imported approved character portraits and twelve comic panels; no Play/build.";
        }
    }
}
