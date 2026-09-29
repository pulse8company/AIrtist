using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airtist.Prototype.Editor
{
    // No Play, real saves, advertising, purchases, or audible playback.
    public static class AirtistRestorationVerification
    {
        public static string Verify()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Static checks require Edit mode.");
            var reports=new List<string>();
            var cue=new AirtistCompletionCue();
            Assert(!cue.Consume(0),"Historic results must be silent");
            cue.Arm(0,"attempt-a");Assert(!cue.Consume(1),"Wrong painting consumed cue");
            Assert(cue.Consume(0) && !cue.Consume(0),"Cue did not consume exactly once");
            cue.Arm(0,"attempt-a");Assert(!cue.Consume(0),"Repeated completion rearmed cue");
            cue.Arm(0,"attempt-b");Assert(cue.Consume(0),"A new replay attempt did not get a cue");
            cue.Arm(1,null);Assert(!cue.Consume(1),"Missing attempt produced a cue");
            reports.Add("Completion cue: once per attempt; historical/reopened/missing attempt silent; new replay allowed");
            string before=PlayerPrefs.GetString(AirtistProgress.SaveKey,"");
            var scene=EditorSceneManager.NewPreviewScene();
            var type=typeof(AirtistLandscapePrototypeController);var flags=BindingFlags.NonPublic|BindingFlags.Instance;
            try
            {
                var original=UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
                var root=new GameObject("ResultVerification",typeof(RectTransform),typeof(Canvas));
                SceneManager.MoveGameObjectToScene(root,scene);((RectTransform)root.transform).sizeDelta=new Vector2(1600,720);
                foreach(bool assisted in new[]{false,true})
                {
                    var copy=UnityEngine.Object.Instantiate(original,root.transform,false);
                    try
                    {
                        Call("LoadProgress");Call("BuildInterface");
                        Set("selectedChapter",0);Set("tutorialStage",-1);Set("galleryOpen",true);
                        Set("appPaused",false);Set("appUnfocused",false);Set("adPending",false);Set("energy",50);Set("coins",0);
                        Set("energyUpdatedUtc",DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                        var found=(bool[][])Get("artifactFound");
                        for(int i=0;i<found[0].Length;i++)found[0][i]=i<found[0].Length-1;
                        ((bool[])Get("chapterCollected"))[0]=false;((bool[])Get("replaying"))[0]=false;
                        ((bool[])Get("economyRewardGranted"))[0]=false;
                        ((AirtistRestorationRecord[])Get("restorations"))[0]=null;
                        var attempt=AirtistAttemptState.Start(new AirtistAttemptRules.PaintingRule());
                        attempt.elapsedSeconds=60;attempt.checksUsed=3;
                        ((AirtistAttemptState[])Get("attempts"))[0]=attempt;
                        if(assisted)Call("CompleteAssistedObject",found[0].Length-1,true);
                        else Call("FindArtworkArtifact",0,found[0].Length-1);
                        var pages=(System.Collections.IDictionary)Get("pages");GameObject result=null;
                        foreach(System.Collections.DictionaryEntry entry in pages)if(entry.Key.ToString()=="Found")result=(GameObject)entry.Value;
                        Assert(result!=null && result.activeSelf,"Completion did not open results");
                        var record=((AirtistRestorationRecord[])Get("restorations"))[0];
                        Assert(record!=null && record.lastMask==7,"Three-star result not recorded");
                        Assert(((AirtistCompletionCue)Get("restorationCue")).Consume(0)==false,"Result page did not consume cue");
                        int coins=(int)Get("coins"),energy=(int)Get("energy");
                        Call("RefreshRestorationScreen");Call("OpenFoundForCurrentChapter");
                        Assert(coins==(int)Get("coins") && energy==(int)Get("energy"),"Reopening grants duplicate rewards");
                        Assert(!((AirtistCompletionCue)Get("restorationCue")).Consume(0),"Reopening rearms sound");
                        Assert(copy.GetComponentsInChildren<AudioSource>(true).Length==0,"Edit check started playback");
                        foreach(int mask in new[]{1,3,7})
                        {
                            record.lastMask=mask;Call("RefreshRestorationScreen");
                            var stars=(AirtistRatingStar[])Get("resultStars");
                            for(int i=0;i<3;i++)Assert((stars[i].color.r>.85f)==((mask&(1<<i))!=0),"Wrong visible star state");
                        }
                        foreach(var graphic in result.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
                            if(graphic.GetComponent<UnityEngine.UI.Button>()==null)Assert(!graphic.raycastTarget,"Decoration blocks result button: "+graphic.name);
                        foreach(var button in result.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                            Assert(button.image!=null && button.image.raycastTarget,"Result button cannot receive taps");
                        var collected=(bool[])Get("chapterCollected");
                        for(int i=0;i<collected.Length;i++)
                        {collected[i]=true;for(int j=0;j<found[i].Length;j++)found[i][j]=true;}
                        Call("RefreshRestorationScreen");
                        Assert(!((UnityEngine.UI.Button)Get("resultNext")).gameObject.activeSelf,"Next painting remains visible after all content is complete");
                        Assert(((UnityEngine.UI.Button)Get("resultCollection")).gameObject.activeSelf,"Gallery action missing on last painting");
                        reports.Add((assisted?"Exact brush":"Normal check")+": result opens, rewards once, cue consumed, star states correct, buttons non-blocked, last painting handled");
                        object Get(string n)=>type.GetField(n,flags).GetValue(copy);
                        void Set(string n,object v)=>type.GetField(n,flags).SetValue(copy,v);
                        object Call(string n,params object[] args)=>type.GetMethod(n,flags).Invoke(copy,args);
                    }
                    finally{UnityEngine.Object.DestroyImmediate(copy.gameObject);}
                }
                var settings=AirtistSettingsConfig.Current;
                Assert(settings!=null && settings.paintingComplete!=null,"Completion sound not assigned");
                reports.Add("User completion audio assigned; real profile unchanged; no Play or audio playback");
                return string.Join("\n",reports);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
                Assert(before==PlayerPrefs.GetString(AirtistProgress.SaveKey,""),"Live progress changed");
            }
        }
        private static void Assert(bool valid,string message){if(!valid)throw new InvalidOperationException(message);}
    }
}
