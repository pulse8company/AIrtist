using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistApprovedThemeSetup
    {
        private const string Art="Assets/Art/ApprovedUI/";
        public static string Import()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before applying artwork.");
            foreach(string n in new[]{"Icons","Surface","HomeBackdrop","MuseumBackdrop","Amelie"})
            {
                string path=Art+n+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=(n=="Icons"||n=="Surface")?SpriteImportMode.Multiple:SpriteImportMode.Single;
                importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;
                importer.maxTextureSize=n=="Surface"?512:n=="Amelie"?1024:2048;
                importer.textureCompression=TextureImporterCompression.Compressed;
                var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=importer.maxTextureSize;android.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
                if(n!="Icons" && n!="Surface")continue;
                importer.GetSourceTextureWidthAndHeight(out int w,out int h);
                var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
                var capabilities=provider.GetDataProvider<ISpriteFrameEditCapability>();
                if(capabilities==null || !capabilities.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite) || !capabilities.GetEditCapability().HasCapability(EEditCapability.EditBorder))throw new InvalidOperationException("Sprite slicing capability missing.");
                var old=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
                var rects=new SpriteRect[n=="Icons"?16:1];
                for(int i=0;i<rects.Length;i++)
                {
                    var name=n=="Icons"?"Icon"+i:"Surface";
                    rects[i]=new SpriteRect{name=name,spriteID=old.TryGetValue(name,out var id)?id:GUID.Generate(),pivot=Vector2.one*.5f,alignment=SpriteAlignment.Center,
                        rect=n=="Icons"?new Rect((i%4)*w/4f,(3-i/4)*h/4f,w/4f,h/4f):new Rect(w*.065f,h*.07f,w*.87f,h*.86f),
                        border=n=="Surface"?new Vector4(w*.15f,h*.16f,w*.15f,h*.15f):Vector4.zero};
                }
                provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
                provider.Apply();importer.SaveAndReimport();
            }
            const string asset="Assets/Resources/ApprovedTheme.asset";
            var theme=AssetDatabase.LoadAssetAtPath<AirtistApprovedTheme>(asset);
            if(theme==null){theme=ScriptableObject.CreateInstance<AirtistApprovedTheme>();AssetDatabase.CreateAsset(theme,asset);}
            theme.surface=AssetDatabase.LoadAllAssetsAtPath(Art+"Surface.png").OfType<Sprite>().First();
            var icons=AssetDatabase.LoadAllAssetsAtPath(Art+"Icons.png").OfType<Sprite>().ToDictionary(s=>s.name);
            theme.icons=Enumerable.Range(0,16).Select(i=>icons["Icon"+i]).ToArray();
            theme.amelie=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Amelie.png");theme.homeBackdrop=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"HomeBackdrop.png");theme.museumBackdrop=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"MuseumBackdrop.png");
            theme.font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(AirtistLocalizationBuilder.FontPath);
            EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();return "Imported theme and 16 individual icons; Android ASTC compression configured.";
        }
        public static string ApplyPrefabs()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first.");
            var theme=AirtistApprovedTheme.Current;if(theme==null)throw new InvalidOperationException("Import theme first.");
            foreach(string path in new[]{AirtistGameplayBuilder.Path,AirtistMyGalleryBuilder.CardPath,AirtistMyGalleryBuilder.ScreenPath,"Assets/UI/Store/StoreScreen.prefab"})
            {
                var root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    theme.Common(root.transform);
                    var gameplay=root.GetComponent<AirtistGameplayScreen>();if(gameplay!=null)theme.Gameplay(gameplay);
                    var gallery=root.GetComponent<AirtistMyGalleryScreen>();if(gallery!=null)theme.Gallery(gallery);
                    foreach(var card in root.GetComponentsInChildren<AirtistGalleryCard>(true))theme.Card(card);
                    var store=root.GetComponent<AirtistStoreScreen>();if(store!=null)theme.Store(store);
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            AssetDatabase.SaveAssets();return "Styled existing gameplay, gallery/card and store prefabs in place. Home/shared navigation and generated windows use the same theme on initialization. No Play/build.";
        }
    }
}
