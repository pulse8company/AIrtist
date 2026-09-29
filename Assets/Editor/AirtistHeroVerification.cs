using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airtist.Prototype.Editor
{
    // Isolated edit-mode check: never plays, never writes the real player profile.
    public static class AirtistHeroVerification
    {
        public static string Verify()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before running the isolated check.");
            const string sessionKey="AIrtist.Progress.TestKey";
            string previousKey=SessionState.GetString(sessionKey,AirtistProgress.SaveKey);
            string testKey="AIrtist.HeroVerification."+Guid.NewGuid().ToString("N");
            string liveBefore=PlayerPrefs.GetString(AirtistProgress.SaveKey,"");
            var scene=EditorSceneManager.NewPreviewScene();
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var type=typeof(AirtistLandscapePrototypeController);
            var report=new List<string>();
            try
            {
                var original=UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
                var root=new GameObject("HeroVerification",typeof(RectTransform),typeof(Canvas));
                SceneManager.MoveGameObjectToScene(root,scene);
                ((RectTransform)root.transform).sizeDelta=new Vector2(1600,720);
                SessionState.SetString(sessionKey,testKey);
                if(!string.IsNullOrEmpty(liveBefore))PlayerPrefs.SetString(testKey,liveBefore);
                var copy=UnityEngine.Object.Instantiate(original,root.transform,false);
                Call(copy,"LoadProgress");Call(copy,"BuildInterface");
                foreach(var finish in copy.GetComponentsInChildren<AirtistButtonFinish>(true))
                    Assert(!finish.raycastTarget,"Button decoration blocks touch input");
                type.GetField("initialized",flags).SetValue(copy,true);
                Call(copy,"SaveProgress");
                var baseline=AirtistProgress.Load();
                for(int hero=0;hero<3;hero++)
                {
                    Call(copy,"OpenIntroduction",true);
                    var buttons=(UnityEngine.UI.Button[])type.GetField("heroButtons",flags).GetValue(copy);
                    buttons[hero].onClick.Invoke();
                    Assert(AirtistProgress.Load().hero==hero,"Portrait tap did not persist hero "+hero);
                    var label=(TMPro.TMP_Text)type.GetField("heroConfirmLabel",flags).GetValue(copy);
                    label.GetComponentInParent<UnityEngine.UI.Button>().onClick.Invoke();
                    Assert(!(bool)type.GetProperty("IntroductionOpen",flags).GetValue(copy),"Confirm did not close the picker");
                    Assert(((RectTransform)type.GetField("universalHeader",flags).GetValue(copy)).gameObject.activeSelf,"Header not restored");
                    CheckProfile(baseline,AirtistProgress.Load());
                    CheckPortraits(copy,hero);
                    // A fresh controller reads the persisted profile, just as at the next app start.
                    var next=UnityEngine.Object.Instantiate(original,root.transform,false);
                    try
                    {
                        Call(next,"LoadProgress");Call(next,"BuildInterface");
                        Assert((int)type.GetField("selectedHero",flags).GetValue(next)==hero,"Hero was not restored on reload");
                        CheckPortraits(next,hero);
                    }
                    finally{UnityEngine.Object.DestroyImmediate(next.gameObject);}
                    report.Add("Hero "+hero+": tap, save, confirm, reload, all portraits OK");
                }
                Call(copy,"OpenIntroduction",true);Call(copy,"StartHeroStory");Call(copy,"FinishIntroduction");
                CheckProfile(baseline,AirtistProgress.Load());report.Add("Story replay preserves route, tutorial, collection and balances");
                var invalid=new AirtistProgress{hero=99,introVersion=1};
                Call(copy,"LoadIntroduction",invalid);
                Assert((int)type.GetField("selectedHero",flags).GetValue(copy)==2,"Invalid index was not clamped");
                report.Add("Invalid stored hero index handled");
                var audio=copy.GetComponent<AirtistMenuAudio>();
                Assert(audio!=null && copy.GetComponents<AirtistMenuAudio>().Length==1,"Duplicate/missing menu audio service");
                var config=AirtistSettingsConfig.Current;
                Assert(config!=null && config.menuClick!=null,"Missing click audio asset");
                var sample=((UnityEngine.UI.Button[])type.GetField("heroButtons",flags).GetValue(copy))[0];
                for(int repeat=0;repeat<3;repeat++)AirtistMenuAudio.Wire(sample);
                var prepare=typeof(UnityEngine.Events.UnityEventBase).GetMethod("PrepareInvoke",BindingFlags.Instance|BindingFlags.NonPublic);
                var calls=(System.Collections.IEnumerable)prepare.Invoke(sample.onClick,null);int audioCalls=0;
                foreach(var call in calls)
                {
                    var field=call.GetType().GetField("Delegate",BindingFlags.Instance|BindingFlags.NonPublic);
                    var callback=field?.GetValue(call) as Delegate;
                    if(callback==null)continue;
                    foreach(var action in callback.GetInvocationList())if(action.Target==audio)audioCalls++;
                }
                Assert(audioCalls==1,"Repeated styling duplicated menu click callback");
                Assert(copy.GetComponentsInChildren<AudioSource>(true).Length==0,"Static preview unexpectedly played audio");
                report.Add("Menu click configured, one callback after repeated wiring; edit-mode silent; decorations do not block input");
                return string.Join("\n",report);

                object Call(object target,string method,params object[] args)=>type.GetMethod(method,flags).Invoke(target,args);
                void CheckPortraits(AirtistLandscapePrototypeController target,int hero)
                {
                    var art=Resources.Load<AirtistIntroArt>("IntroductionArt");int count=0;
                    foreach(var image in target.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                        if(image.name=="ApprovedAmelie"){Assert(image.enabled && image.sprite==art.Hero(hero),"Wrong portrait on "+image.transform.parent.name);count++;}
                    Assert(count>=4,"Missing expected home/gameplay/gallery/store portrait slots");
                }
                void CheckProfile(AirtistProgress a,AirtistProgress b)
                {
                    Assert(a.selectedChapter==b.selectedChapter && a.tutorialStage==b.tutorialStage && a.tutorialVersion==b.tutorialVersion,"Changing hero changed route/tutorial");
                    Assert(a.coins==b.coins && a.energy==b.energy && a.hints==b.hints && a.socialRewardsClaimed==b.socialRewardsClaimed,"Changing hero changed balances");
                    Assert(a.chapters.Length==b.chapters.Length,"Changed chapter count");
                    for(int i=0;i<a.chapters.Length;i++)Assert(JsonUtility.ToJson(a.chapters[i])==JsonUtility.ToJson(b.chapters[i]),"Changed painting progress "+i);
                }
            }
            finally
            {
                SessionState.SetString(sessionKey,previousKey);
                PlayerPrefs.DeleteKey(testKey);PlayerPrefs.Save();
                EditorSceneManager.ClosePreviewScene(scene);
                Assert(liveBefore==PlayerPrefs.GetString(AirtistProgress.SaveKey,""),"Real player profile was modified");
            }
        }
        private static void Assert(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
    }
}
