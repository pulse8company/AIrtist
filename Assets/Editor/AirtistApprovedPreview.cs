using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airtist.Prototype.Editor
{
    // Renders an isolated UI copy. No Play, button invocations, timer ticks, or progress writes.
    public static class AirtistApprovedPreview
    {
        public static string Render(string screen,int width=1672,int height=941,int insetLeft=0,int insetRight=0,int hero=-1)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Static preview requires Edit mode.");
            var original=UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            var scene=EditorSceneManager.NewPreviewScene();RenderTexture target=null;Texture2D shot=null;Camera camera=null;
            try
            {
                var cameraObject=new GameObject("PreviewCamera",typeof(Camera));SceneManager.MoveGameObjectToScene(cameraObject,scene);
                camera=cameraObject.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=height/2f;camera.transform.position=new Vector3(0,0,-1000);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.gray;camera.nearClipPlane=.1f;camera.farClipPlane=3000;camera.cullingMask=1<<30;
                camera.scene=scene;
                target=new RenderTexture(width,height,24);camera.targetTexture=target;
                var canvasObject=new GameObject("PreviewCanvas",typeof(RectTransform),typeof(Canvas));SceneManager.MoveGameObjectToScene(canvasObject,scene);
                var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;
                var canvasRect=(RectTransform)canvas.transform;canvasRect.sizeDelta=new Vector2(width,height);
                var copy=UnityEngine.Object.Instantiate(original,canvas.transform,false);copy.name="StaticPreview";AirtistFlexibleArtboard.Fill((RectTransform)copy.transform);
                ((RectTransform)copy.transform).offsetMin=new Vector2(insetLeft,0);((RectTransform)copy.transform).offsetMax=new Vector2(-insetRight,0);
                var flags=BindingFlags.NonPublic|BindingFlags.Instance;var type=typeof(AirtistLandscapePrototypeController);
                type.GetMethod("LoadProgress",flags).Invoke(copy,null);
                if(hero>=0)type.GetField("selectedHero",flags).SetValue(copy,Mathf.Clamp(hero,0,2));
                bool tutorialPreview=screen.StartsWith("Tutorial",StringComparison.Ordinal);
                bool monaPreview=tutorialPreview || screen=="MonaLisa";
                bool resultPreview=screen.StartsWith("Result",StringComparison.Ordinal);
                if(resultPreview)
                {
                    // Synthetic finished attempt on this copy; never persisted or rewarded.
                    type.GetField("selectedChapter",flags).SetValue(copy,0);
                    var found=(bool[][])type.GetField("artifactFound",flags).GetValue(copy);
                    for(int i=0;i<found[0].Length;i++)found[0][i]=true;
                    ((bool[])type.GetField("chapterCollected",flags).GetValue(copy))[0]=true;
                    int mask=screen=="ResultOneStar"?1:screen=="ResultTwoStars"?3:7;
                    bool repeat=screen=="ResultReplay",legacy=screen=="ResultLegacy";
                    ((AirtistRestorationRecord[])type.GetField("restorations",flags).GetValue(copy))[0]=new AirtistRestorationRecord{
                        lastMask=legacy?1:mask,bestMask=legacy?1:mask,measured=!legacy,seconds=mask==1?200:68,checks=mask==7?4:8,
                        coinsGranted=repeat||legacy?0:mask==7?40:35,energyGranted=repeat||legacy?0:8,paidStarMask=repeat?6:mask&6};
                }
                if(monaPreview)
                {
                    type.GetField("selectedChapter",flags).SetValue(copy,0);
                    var found=(bool[][])type.GetField("artifactFound",flags).GetValue(copy);
                    Array.Clear(found[0],0,found[0].Length);
                    ((bool[])type.GetField("chapterCollected",flags).GetValue(copy))[0]=false;
                    int stage=0;
                    if(tutorialPreview && !int.TryParse(screen.Substring(8),out stage))stage=0;
                    type.GetField("tutorialStage",flags).SetValue(copy,tutorialPreview?Mathf.Clamp(stage,0,8):-1);
                }
                // initialized remains false; SaveProgress is guarded and cannot write.
                type.GetMethod("BuildInterface",flags).Invoke(copy,null);
                var pages=(IDictionary)type.GetField("pages",flags).GetValue(copy);
                object selected=null;
                string pageName=resultPreview?"Found":monaPreview?"Gallery":screen.StartsWith("Settings",StringComparison.Ordinal)?"Collection":screen=="WorldMapEurope"?"WorldMap":screen=="NoAds"?"Store":screen=="Energy"||screen=="Characters"||screen=="Story"?"Home":screen;
                foreach(DictionaryEntry pair in pages){bool visible=pair.Key.ToString()==pageName;((GameObject)pair.Value).SetActive(visible);if(visible)selected=pair.Key;}
                if(selected==null)throw new ArgumentException("Unknown screen: "+screen);
                type.GetMethod("UpdateFullscreenBackground",flags).Invoke(copy,new[]{selected});
                var header=(RectTransform)type.GetField("universalHeader",flags).GetValue(copy);header.SetAsLastSibling();
                foreach(string n in new[]{"universalHome","universalClose"})((GameObject)type.GetField(n,flags).GetValue(copy)).SetActive(screen!="Home");
                if(screen=="Found" || resultPreview)type.GetMethod("RefreshRestorationScreen",flags).Invoke(copy,null);
                if(screen=="MuseumPreview")type.GetMethod("OpenMuseumPreview",flags).Invoke(copy,new object[]{0});
                if(screen=="NoAds")copy.GetComponentInChildren<AirtistStoreScreen>(true).OpenProduct(0);
                type.GetField("galleryOpen",flags).SetValue(copy,pageName=="Gallery");
                if(monaPreview)type.GetMethod("RefreshTutorial",flags).Invoke(copy,null);
                type.GetMethod("BringDeveloperResetToFront",flags)?.Invoke(copy,null);
                if(screen.StartsWith("Settings",StringComparison.Ordinal))
                {
                    type.GetMethod("OpenSettings",flags).Invoke(copy,null);
                    if(screen=="SettingsAds")type.GetMethod("OpenAdSettings",flags).Invoke(copy,null);
                    if(screen=="SettingsLanguage")type.GetMethod("OpenLanguageSettings",flags).Invoke(copy,null);
                    if(screen=="SettingsSupport")type.GetMethod("OpenSettingsSupport",flags).Invoke(copy,null);
                }
                if(screen=="Energy")
                {
                    // Synthetic display values on the isolated copy only, never saved.
                    type.GetField("energy",flags).SetValue(copy,0);type.GetField("coins",flags).SetValue(copy,100);
                    type.GetField("energyUpdatedUtc",flags).SetValue(copy,DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    type.GetMethod("OpenEnergyShop",flags).Invoke(copy,null);
                    type.GetMethod("RefreshHeaderWallet",flags).Invoke(copy,null);
                }
                if(screen=="Characters" || screen=="Story")
                {
                    type.GetMethod("OpenIntroduction",flags).Invoke(copy,new object[]{true});
                    if(screen=="Story")
                    {
                        type.GetField("introFrame",flags).SetValue(copy,2);
                        type.GetMethod("RefreshStory",flags).Invoke(copy,null);
                    }
                }
                foreach(var t in canvasObject.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
                foreach(var l in canvasObject.GetComponentsInChildren<AirtistScaledLabel>())if(l.isActiveAndEnabled)RefreshForPreview(l);
                foreach(var g in canvasObject.GetComponentsInChildren<AirtistGalleryGrid>(true))g.Refresh();
                Canvas.ForceUpdateCanvases();
                foreach(var map in canvasObject.GetComponentsInChildren<AirtistWorldMapScreen>())map.RefreshLayout();
                if(screen=="WorldMapEurope")
                    foreach(var map in canvasObject.GetComponentsInChildren<AirtistWorldMapScreen>())map.FocusOn(new Vector2(.46f,.75f),4);
                foreach(var markers in canvasObject.GetComponentsInChildren<AirtistMuseumMapLayout>())markers.Refresh();
                foreach(var bleed in canvasObject.GetComponentsInChildren<AirtistHeaderBleed>())bleed.Refresh();
                Canvas.ForceUpdateCanvases();
                foreach(var cue in canvasObject.GetComponentsInChildren<AirtistTutorialCue>())
                    if(cue.isActiveAndEnabled)RefreshForPreview(cue);
                foreach(var layout in canvasObject.GetComponentsInChildren<AirtistHiddenObjectLayout>())
                    if(layout.isActiveAndEnabled)RefreshForPreview(layout);
                foreach(var text in canvasObject.GetComponentsInChildren<TMPro.TMP_Text>())text.ForceMeshUpdate();
                foreach(var finish in canvasObject.GetComponentsInChildren<AirtistButtonFinish>())finish.Refresh();
                camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;shot=new Texture2D(width,height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,width,height),0,0);shot.Apply();RenderTexture.active=previous;
                string folder="ArtReview/ApprovedImplementation";Directory.CreateDirectory(folder);string path=folder+"/"+screen+(hero>=0?"-hero"+hero:"")+"-"+width+"x"+height+(insetLeft>0||insetRight>0?"-safe":"")+".png";File.WriteAllBytes(path,shot.EncodeToPNG());return Path.GetFullPath(path);
            }
            finally{if(camera!=null)camera.targetTexture=null;if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}if(shot!=null)UnityEngine.Object.DestroyImmediate(shot);EditorSceneManager.ClosePreviewScene(scene);}
        }
        private static void RefreshForPreview(Component component)
        {
            // Preview-scene behaviours do not run normal Unity messages. Refresh layout explicitly.
            component.GetType().GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic)?.Invoke(component,null);
        }
    }
}
