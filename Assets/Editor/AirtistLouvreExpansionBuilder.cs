using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistLouvreExpansionBuilder
    {
        const string GalleryPath="Assets/UI/MyGallery/GalleryCatalog.asset";
        const string SheetPath="Assets/Art/Prototype/Gameplay/LouvreObjects.png";
        public static void Connect(AirtistLandscapePrototypeController controller)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<AirtistGalleryCatalog>(GalleryPath);
            if(catalog==null || !catalog.paintings.Any(p=>p.id=="cana")) return;
            var serialized=new SerializedObject(controller);
            var paintings=serialized.FindProperty("louvrePaintings");
            paintings.arraySize=Math.Max(paintings.arraySize,AirtistLandscapePrototypeController.PaintingCount);
            foreach(var painting in catalog.paintings.Where(p=>p.chapterIndex>=3 && p.chapterIndex<AirtistLandscapePrototypeController.PaintingCount))
                paintings.GetArrayElementAtIndex(painting.chapterIndex).objectReferenceValue=painting.artwork;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static string Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            var controller=UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            if(controller==null || controller.gameObject.scene.path!="Assets/Scenes/AirtistLandscapePrototype.unity")
                throw new InvalidOperationException("Open the prototype scene first.");
            var gallery=AssetDatabase.LoadAssetAtPath<AirtistGalleryCatalog>(GalleryPath);
            var objects=AssetDatabase.LoadAssetAtPath<AirtistHiddenObjectCatalog>(AirtistHiddenObjectBuilder.CatalogPath);
            var rules=AssetDatabase.LoadAssetAtPath<AirtistAttemptRules>("Assets/UI/Gameplay/AttemptRules.asset");
            if(gallery==null || objects==null || rules==null) throw new InvalidOperationException("Existing catalogs required.");
            var sprites=Slice();
            var paintings=gallery.paintings.ToList();
            var entries=objects.entries.ToList();
            var attempts=rules.paintings.ToList();
            for(int i=0;i<AirtistLouvreExpansion.Levels.Length;i++)
            {
                var level=AirtistLouvreExpansion.Levels[i];
                var art=ImportPainting(level.file);
                if(!paintings.Any(p=>p.id==level.id)) paintings.Add(new AirtistGalleryCatalog.Painting {
                    id=level.id,title=level.title,artist=level.artist,museum=0,artwork=art,chapterIndex=i+3 });
                for(int j=0;j<level.targets.Length;j++)
                {
                    var target=level.targets[j];
                    if(entries.Any(e=>e.chapterId==level.id && e.artifactIndex==j)) continue;
                    entries.Add(new AirtistHiddenObjectCatalog.Entry { chapterId=level.id,artifactIndex=j,
                        title=target.name,sprite=sprites[target.sprite],position=target.point,
                        widthFraction=target.width,rotation=target.rotation,
                        tint=level.id=="astronomer"?new Color(.67f,.66f,.61f):new Color(.88f,.84f,.77f) });
                }
                if(!attempts.Any(r=>r.chapterId==level.id)) attempts.Add(new AirtistAttemptRules.PaintingRule {
                    chapterId=level.id,mode=AirtistAttemptMode.ClicksAndTime,clicks=level.clicks,seconds=level.seconds,
                    continuationClicks=3,continuationSeconds=45 });
            }
            gallery.paintings=paintings.ToArray(); objects.entries=entries.ToArray(); rules.paintings=attempts.ToArray();
            EditorUtility.SetDirty(gallery); EditorUtility.SetDirty(objects); EditorUtility.SetDirty(rules);
            Connect(controller);
            EditorUtility.SetDirty(controller); EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            UpdateRouteSummary();
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(controller.gameObject.scene);
            return Validate();
        }

        public static void UpdateRouteSummary()
        {
            var collection=UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(AirtistLocalizationBuilder.Table);
            var table=collection?.GetTable("ru") as UnityEngine.Localization.Tables.StringTable;
            var entry=table?.GetEntry("home.route.summary");
            if(entry==null) throw new InvalidOperationException("Missing existing Home route localization key.");
            entry.Value=AirtistLandscapePrototypeController.RouteSummary;
            EditorUtility.SetDirty(table); EditorUtility.SetDirty(collection); EditorUtility.SetDirty(collection.SharedData);
            UnityEditor.Localization.LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null,collection);
            const string path="Assets/UI/Home/HomeScreen.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var label=root.transform.Find("Text_home.route.summary");
                if(label==null) throw new InvalidOperationException("Home summary label not found.");
                label.GetComponent<TMPro.TMP_Text>().text=entry.Value;
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        static Sprite ImportPainting(string file)
        {
            string path="Assets/Art/Prototype/Louvre/"+file+".jpg";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            importer.mipmapEnabled=false; importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=2048; importer.isReadable=false;
            importer.textureCompression=TextureImporterCompression.CompressedHQ; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? throw new InvalidOperationException("Missing artwork "+file);
        }

        static Sprite[] Slice()
        {
            AssetDatabase.ImportAsset(SheetPath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(SheetPath);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true; importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None; importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            var factory=new SpriteDataProviderFactories(); factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);
            if(provider==null) throw new InvalidOperationException("Sprite provider unavailable.");
            provider.InitSpriteEditorDataProvider();
            var edit=provider.GetDataProvider<ISpriteFrameEditCapability>();
            if(edit==null || !edit.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite)
                || !edit.GetEditCapability().HasCapability(EEditCapability.EditSpriteRect))
                throw new InvalidOperationException("Sprite slicing unsupported.");
            var previous=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
            var source=new Texture2D(2,2);
            SpriteRect[] rects;
            try
            {
                ImageConversion.LoadImage(source,System.IO.File.ReadAllBytes(SheetPath));
                if(source.width!=1254 || source.height!=1254) throw new InvalidOperationException("Unexpected sheet dimensions.");
                var pixels=source.GetPixels32();
                if(!pixels.Any(p=>p.a==0)) throw new InvalidOperationException("Transparent sheet required.");
                int[] xs={0,335,645,970,1254}, ys={0,475,845,1254};
                rects=Enumerable.Range(0,12).Select(i=> {
                    int left=1254,right=-1,bottom=1254,top=-1,col=i%4,row=i/4;
                    for(int y=1254-ys[row+1];y<1254-ys[row];y++) for(int x=xs[col];x<xs[col+1];x++)
                        if(pixels[y*1254+x].a>16) { left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y); }
                    if(right<left) throw new InvalidOperationException("Empty sprite "+i);
                    string name="LouvreObject"+i.ToString("00");
                    return new SpriteRect { name=name,spriteID=previous.TryGetValue(name,out var id)?id:GUID.Generate(),
                        rect=new Rect(left,bottom,right-left+1,top-bottom+1),pivot=Vector2.one*.5f,alignment=SpriteAlignment.Center };
                }).ToArray();
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var loaded=AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>().ToDictionary(s=>s.name);
            return Enumerable.Range(0,12).Select(i=>loaded["LouvreObject"+i.ToString("00")]).ToArray();
        }

        public static string Validate()
        {
            var gallery=AssetDatabase.LoadAssetAtPath<AirtistGalleryCatalog>(GalleryPath);
            var objects=AssetDatabase.LoadAssetAtPath<AirtistHiddenObjectCatalog>(AirtistHiddenObjectBuilder.CatalogPath);
            var rules=AssetDatabase.LoadAssetAtPath<AirtistAttemptRules>("Assets/UI/Gameplay/AttemptRules.asset");
            var controller=UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            var serialized=new SerializedObject(controller);
            var art=serialized.FindProperty("louvrePaintings");
            foreach(var p in gallery.paintings)
            {
                if(p.artwork==null || p.chapterIndex<0 || p.chapterIndex>=art.arraySize
                    || art.GetArrayElementAtIndex(p.chapterIndex).objectReferenceValue!=p.artwork)
                    throw new InvalidOperationException("Unconnected artwork: "+p.id);
                if(!rules.paintings.Any(r=>r.chapterId==p.id)) throw new InvalidOperationException("Missing attempt rule: "+p.id);
            }
            foreach(var level in AirtistLouvreExpansion.Levels)
            {
                for(int i=0;i<level.targets.Length;i++)
                    if(objects.Find(level.id,i)?.sprite==null) throw new InvalidOperationException("Missing target: "+level.id+"/"+i);
                // Conservative base-zoom target rectangles, in the existing 1672-wide design canvas.
                // Larger visual bounds include rotation; phone/tablet behavior still needs manual Play checks.
                var painting=gallery.paintings.Single(p=>p.id==level.id).artwork;
                float aspect=painting.rect.width/painting.rect.height;
                float width=Math.Min(1314,690*aspect),height=width/aspect;
                var targets=objects.entries.Where(e=>e.chapterId==level.id).ToArray();
                var bounds=targets.Select(e=> {
                    float w=width*e.widthFraction,h=w*e.sprite.rect.height/e.sprite.rect.width;
                    float angle=e.rotation*Mathf.Deg2Rad;
                    float rw=Mathf.Abs(w*Mathf.Cos(angle))+Mathf.Abs(h*Mathf.Sin(angle));
                    float rh=Mathf.Abs(w*Mathf.Sin(angle))+Mathf.Abs(h*Mathf.Cos(angle));
                    var size=new Vector2(Mathf.Max(72,rw+18),Mathf.Max(64,rh+18));
                    return new Rect(new Vector2(width*e.position.x,height*e.position.y)-size*.5f,size);
                }).ToArray();
                for(int a=0;a<bounds.Length;a++) for(int b=a+1;b<bounds.Length;b++)
                    if(bounds[a].Overlaps(bounds[b])) throw new InvalidOperationException("Overlapping targets: "+level.id+"/"+a+"/"+b);
            }
            return $"Paintings={gallery.paintings.Length}; targets={objects.entries.Length}; rules={rules.paintings.Length}; missing references=0. Play/build not run.";
        }
    }
}
