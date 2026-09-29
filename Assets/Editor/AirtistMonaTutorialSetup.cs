using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistMonaTutorialSetup
    {
        public static string ImportBrooch(string source)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first.");
            const string path="Assets/Art/Prototype/Gameplay/MonaBrooch.png";
            if(!File.Exists(path))File.Copy(source,path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=512;
            var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;
            android.maxTextureSize=512;android.format=TextureImporterFormat.ASTC_4x4;importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
            var catalog=AssetDatabase.LoadAssetAtPath<AirtistHiddenObjectCatalog>("Assets/UI/Gameplay/HiddenObjectCatalog.asset");
            if(catalog==null)throw new InvalidOperationException("Existing hidden-object catalog missing.");
            var entries=catalog.entries.ToList();
            var entry=entries.FirstOrDefault(e=>e.chapterId=="mona-lisa" && e.artifactIndex==3);
            if(entry==null){entry=new AirtistHiddenObjectCatalog.Entry{chapterId="mona-lisa",artifactIndex=3};entries.Add(entry);}
            entry.title="Потемневшая брошь";entry.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            entry.position=new Vector2(.665f,.465f);entry.widthFraction=.078f;entry.rotation=-14;
            entry.tint=new Color(.66f,.61f,.50f,1);
            catalog.entries=entries.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
            return "Mona Lisa: four independent targets; fourth is the toned antique brooch.";
        }
    }
}
