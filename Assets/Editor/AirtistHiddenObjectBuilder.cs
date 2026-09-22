using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistHiddenObjectBuilder
    {
        public const string CatalogPath="Assets/UI/Gameplay/HiddenObjectCatalog.asset";
        private const string ArtPath="Assets/Art/Prototype/Gameplay/HiddenObjects.png";
        public static void Connect(AirtistLandscapePrototypeController controller)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<AirtistHiddenObjectCatalog>(CatalogPath);
            if(catalog!=null) controller.ConfigureHiddenObjects(catalog);
        }
        public static string Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            var controller=UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            if(controller==null || controller.gameObject.scene.path!="Assets/Scenes/AirtistLandscapePrototype.unity") throw new InvalidOperationException("Open the prototype scene.");
            var sprites=Slice();
            var catalog=AssetDatabase.LoadAssetAtPath<AirtistHiddenObjectCatalog>(CatalogPath);
            if(catalog==null)
            {
                catalog=ScriptableObject.CreateInstance<AirtistHiddenObjectCatalog>();
                string[] titles={"Усик","Часы","Уточка","Наушники","Стаканчик","Самолётик","Кроссовок","Круг","Бутылка","Мяч","Очки","Конус"};
                Vector2[] points={new Vector2(.46f,.697f),new Vector2(.335f,.24f),new Vector2(.165f,.59f),
                    new Vector2(.75f,.63f),new Vector2(.11f,.265f),new Vector2(.82f,.85f),new Vector2(.59f,.33f),
                    new Vector2(.11f,.23f),new Vector2(.57f,.10f),new Vector2(.90f,.26f),new Vector2(.456f,.413f),new Vector2(.305f,.225f)};
                float[] widths={.14f,.085f,.12f,.07f,.055f,.085f,.085f,.085f,.042f,.072f,.065f,.06f};
                catalog.entries=Enumerable.Range(0,12).Select(i=>new AirtistHiddenObjectCatalog.Entry {
                    chapterId=i<3?"mona-lisa":i<7?"liberty":"medusa", artifactIndex=i<3?i:i<7?i-3:i-7,
                    title=titles[i], sprite=sprites[i], position=points[i], widthFraction=widths[i], rotation=i==1?-25:i==5?-10:0
                }).ToArray();
                AssetDatabase.CreateAsset(catalog,CatalogPath);
            }
            Connect(controller);
            // Give the five-object challenge room for four mistakes on a fresh attempt.
            var rules=AssetDatabase.LoadAssetAtPath<AirtistAttemptRules>("Assets/UI/Gameplay/AttemptRules.asset");
            if(rules!=null)
            {
                var medusa=rules.Get("medusa");
                if(medusa.clicks==5 && medusa.seconds==90) { medusa.clicks=9; medusa.seconds=150; EditorUtility.SetDirty(rules); }
            }
            EditorUtility.SetDirty(controller); EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            EditorSceneManager.SaveScene(controller.gameObject.scene); AssetDatabase.SaveAssets();
            return "12 sprites and editable placements connected across 3/4/5-object paintings. Existing saves preserved. No Play/build.";
        }

        private static Sprite[] Slice()
        {
            AssetDatabase.ImportAsset(ArtPath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(ArtPath);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=2048; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            var factory=new SpriteDataProviderFactories(); factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var edit=provider.GetDataProvider<ISpriteFrameEditCapability>();
            if(edit==null || !edit.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite)
                || !edit.GetEditCapability().HasCapability(EEditCapability.EditSpriteRect)) throw new InvalidOperationException("Sprite slicing capability unavailable.");
            var previous=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
            RectInt[] areas={new RectInt(32,230,304,120),new RectInt(355,85,250,355),new RectInt(630,140,294,288),new RectInt(933,80,292,349),
                new RectInt(42,466,247,332),new RectInt(297,495,320,275),new RectInt(610,533,322,244),new RectInt(933,468,307,332),
                new RectInt(83,807,164,372),new RectInt(310,854,298,291),new RectInt(616,934,357,193),new RectInt(976,809,260,368)};
            var source=new Texture2D(2,2);
            SpriteRect[] rects;
            try
            {
                ImageConversion.LoadImage(source,System.IO.File.ReadAllBytes(ArtPath));
                if(source.width!=1254 || source.height!=1254) throw new InvalidOperationException("Unexpected sprite sheet size.");
                var pixels=source.GetPixels32();
                if(!pixels.Any(p=>p.a==0)) throw new InvalidOperationException("Sprite sheet is not transparent.");
                rects=areas.Select((area,i)=> {
                    int left=source.width,right=-1,bottom=source.height,top=-1;
                    for(int y=source.height-area.yMax;y<source.height-area.yMin;y++) for(int x=area.xMin;x<area.xMax;x++)
                        if(pixels[y*source.width+x].a>16) { left=Math.Min(left,x); right=Math.Max(right,x); bottom=Math.Min(bottom,y); top=Math.Max(top,y); }
                    if(right<left) throw new InvalidOperationException("Empty object region "+i);
                    string name="HiddenObject"+i.ToString("00");
                    return new SpriteRect { name=name, spriteID=previous.TryGetValue(name,out var id)?id:GUID.Generate(),
                        rect=new Rect(left,bottom,right-left+1,top-bottom+1), pivot=Vector2.one*.5f,alignment=SpriteAlignment.Center };
                }).ToArray();
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var loaded=AssetDatabase.LoadAllAssetsAtPath(ArtPath).OfType<Sprite>().ToDictionary(s=>s.name);
            return Enumerable.Range(0,12).Select(i=>loaded["HiddenObject"+i.ToString("00")]).ToArray();
        }

        public static string AddHardObjects()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            var catalog=AssetDatabase.LoadAssetAtPath<AirtistHiddenObjectCatalog>(CatalogPath);
            if(catalog==null) throw new InvalidOperationException("Missing existing object catalog.");
            var coin=ImportHardObject("HardCoin");
            var key=ImportHardObject("HardKey");
            var entries=catalog.entries.ToList();
            if(catalog.Find("liberty",4)==null) entries.Add(new AirtistHiddenObjectCatalog.Entry {
                chapterId="liberty",artifactIndex=4,title="Потемневшая монета",sprite=coin,
                position=new Vector2(.225f,.242f),widthFraction=.029f,rotation=-8 });
            if(catalog.Find("medusa",5)==null) entries.Add(new AirtistHiddenObjectCatalog.Entry {
                chapterId="medusa",artifactIndex=5,title="Старый железный ключ",sprite=key,
                position=new Vector2(.37f,.105f),widthFraction=.054f,rotation=-16 });
            catalog.entries=entries.ToArray(); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            return "Hard coin and key added; other placements and original paintings untouched.";
        }

        private static Sprite ImportHardObject(string name)
        {
            string path="Assets/Art/Prototype/Gameplay/"+name+".png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true; importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None; importer.maxTextureSize=1024;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            var factory=new SpriteDataProviderFactories(); factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var edit=provider.GetDataProvider<ISpriteFrameEditCapability>();
            if(edit==null || !edit.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite)
                || !edit.GetEditCapability().HasCapability(EEditCapability.EditSpriteRect)) throw new InvalidOperationException("Sprite editing unsupported.");
            var source=new Texture2D(2,2); Rect bounds;
            try
            {
                ImageConversion.LoadImage(source,System.IO.File.ReadAllBytes(path));
                var pixels=source.GetPixels32();
                if(!pixels.Any(p=>p.a==0)) throw new InvalidOperationException("Transparent background required.");
                int left=source.width,right=-1,bottom=source.height,top=-1;
                for(int y=0;y<source.height;y++) for(int x=0;x<source.width;x++)
                    if(pixels[y*source.width+x].a>16) { left=Math.Min(left,x); right=Math.Max(right,x); bottom=Math.Min(bottom,y); top=Math.Max(top,y); }
                if(right<left) throw new InvalidOperationException("Empty sprite.");
                bounds=new Rect(left,bottom,right-left+1,top-bottom+1);
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
            var old=provider.GetSpriteRects().FirstOrDefault(r=>r.name==name);
            var id=old!=null?old.spriteID:GUID.Generate();
            provider.SetSpriteRects(new[]{new SpriteRect { name=name,spriteID=id,rect=bounds,pivot=Vector2.one*.5f,alignment=SpriteAlignment.Center }});
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(new[]{new SpriteNameFileIdPair(name,id)});
            provider.Apply(); importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single();
        }
    }
}
