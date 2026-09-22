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
        public static string Render(string screen,int width=1672,int height=941)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Static preview requires Edit mode.");
            var original=UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            var scene=EditorSceneManager.NewPreviewScene();RenderTexture target=null;Texture2D shot=null;
            try
            {
                var cameraObject=new GameObject("PreviewCamera",typeof(Camera));SceneManager.MoveGameObjectToScene(cameraObject,scene);
                var camera=cameraObject.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=height/2f;camera.transform.position=new Vector3(0,0,-1000);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.gray;camera.nearClipPlane=.1f;camera.farClipPlane=3000;camera.cullingMask=1<<30;
                camera.scene=scene;
                target=new RenderTexture(width,height,24);camera.targetTexture=target;
                var canvasObject=new GameObject("PreviewCanvas",typeof(RectTransform),typeof(Canvas));SceneManager.MoveGameObjectToScene(canvasObject,scene);
                var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;
                var canvasRect=(RectTransform)canvas.transform;canvasRect.sizeDelta=new Vector2(width,height);
                var copy=UnityEngine.Object.Instantiate(original,canvas.transform,false);copy.name="StaticPreview";AirtistFlexibleArtboard.Fill((RectTransform)copy.transform);
                var flags=BindingFlags.NonPublic|BindingFlags.Instance;var type=typeof(AirtistLandscapePrototypeController);
                type.GetMethod("LoadProgress",flags).Invoke(copy,null);
                // initialized remains false; SaveProgress is guarded and cannot write.
                type.GetMethod("BuildInterface",flags).Invoke(copy,null);
                var pages=(IDictionary)type.GetField("pages",flags).GetValue(copy);
                object selected=null;
                string pageName=screen=="Energy"?"Home":screen;
                foreach(DictionaryEntry pair in pages){bool visible=pair.Key.ToString()==pageName;((GameObject)pair.Value).SetActive(visible);if(visible)selected=pair.Key;}
                if(selected==null)throw new ArgumentException("Unknown screen: "+screen);
                type.GetMethod("UpdateFullscreenBackground",flags).Invoke(copy,new[]{selected});
                var header=(RectTransform)type.GetField("universalHeader",flags).GetValue(copy);header.SetAsLastSibling();
                foreach(string n in new[]{"universalHome","universalClose"})((GameObject)type.GetField(n,flags).GetValue(copy)).SetActive(screen!="Home");
                if(screen=="Found")type.GetMethod("RefreshRestorationScreen",flags).Invoke(copy,null);
                if(screen=="MuseumPreview")type.GetMethod("OpenMuseumPreview",flags).Invoke(copy,new object[]{0});
                type.GetField("galleryOpen",flags).SetValue(copy,screen=="Gallery");
                type.GetMethod("BringDeveloperResetToFront",flags)?.Invoke(copy,null);
                if(screen=="Energy")
                {
                    // Synthetic display values on the isolated copy only, never saved.
                    type.GetField("energy",flags).SetValue(copy,0);type.GetField("coins",flags).SetValue(copy,100);
                    type.GetField("energyUpdatedUtc",flags).SetValue(copy,DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    type.GetMethod("OpenEnergyShop",flags).Invoke(copy,null);
                    type.GetMethod("RefreshHeaderWallet",flags).Invoke(copy,null);
                }
                foreach(var t in canvasObject.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
                foreach(var l in canvasObject.GetComponentsInChildren<AirtistScaledLabel>(true))l.SendMessage("LateUpdate");
                foreach(var g in canvasObject.GetComponentsInChildren<AirtistGalleryGrid>(true))g.Refresh();
                Canvas.ForceUpdateCanvases();
                foreach(var text in canvasObject.GetComponentsInChildren<TMPro.TMP_Text>())text.ForceMeshUpdate();
                camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;shot=new Texture2D(width,height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,width,height),0,0);shot.Apply();RenderTexture.active=previous;
                string folder="ArtReview/ApprovedImplementation";Directory.CreateDirectory(folder);string path=folder+"/"+screen+"-"+width+"x"+height+".png";File.WriteAllBytes(path,shot.EncodeToPNG());return Path.GetFullPath(path);
            }
            finally{if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}if(shot!=null)UnityEngine.Object.DestroyImmediate(shot);EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
