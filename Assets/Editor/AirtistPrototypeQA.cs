using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Airtist.Prototype;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;

namespace Airtist.Prototype.Editor
{
    /// <summary>Explicitly invoked editor smoke tests. Uses an isolated save slot, never the player's progress.</summary>
    public static class AirtistPrototypeQA
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        public static string LastResult = "Not run";
        private static AirtistLandscapePrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
        private static object Call(string method, params object[] args) => typeof(AirtistLandscapePrototypeController).GetMethod(method, Flags).Invoke(Controller, args);
        private static T Field<T>(string name) => (T)typeof(AirtistLandscapePrototypeController).GetField(name, Flags).GetValue(Controller);
        private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        private static string CurrentPage() => Controller.transform.Cast<Transform>().First(t => t.gameObject.activeSelf).name;
        private static void Show(string page) => Call("Show", Enum.Parse(typeof(AirtistLandscapePrototypeController).GetNestedType("Page", BindingFlags.NonPublic), page));
        public static string Prepare()
        {
            Check(!EditorApplication.isPlaying, "Stop Play before preparing QA");
            SessionState.SetString("AIrtist.Progress.TestKey", "AIrtist.Progress.QA." + Guid.NewGuid().ToString("N"));
            return "Isolated QA save slot selected; player save untouched.";
        }
        public static string Finish()
        {
            Check(!EditorApplication.isPlaying, "Stop Play before restoring player slot");
            string key = SessionState.GetString("AIrtist.Progress.TestKey", "");
            if (key.StartsWith("AIrtist.Progress.QA.", StringComparison.Ordinal)) { PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
            SessionState.EraseString("AIrtist.Progress.TestKey");
            return "Player slot restored; temporary QA slot removed.";
        }
        public static string Run(int phase)
        {
            Check(EditorApplication.isPlaying, "Enter Play first");
            Check(SessionState.GetString("AIrtist.Progress.TestKey", "").StartsWith("AIrtist.Progress.QA."), "QA needs an isolated slot");
            LastResult = "Running";
            Controller.StartCoroutine(Guard(phase));
            return LastResult;
        }
        private static IEnumerator Guard(int phase)
        {
            var test = Tests(phase);
            while (true)
            {
                bool more;
                try { more = test.MoveNext(); }
                catch (Exception ex) { LastResult = "FAIL: " + ex; File.WriteAllText("Library/AIrtistQA.txt", LastResult); yield break; }
                if (!more) break;
                yield return test.Current;
            }
            LastResult = "PASS phase " + phase;
            File.WriteAllText("Library/AIrtistQA.txt", LastResult);
        }
        private static IEnumerator Tests(int phase)
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            if (phase == 1)
            {
                Check(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1, "Exactly one EventSystem required");
                var home = UnityEngine.Object.FindFirstObjectByType<AirtistHomeScreen>();
                Check(home.GetComponentsInChildren<LocalizeStringEvent>().Length == 17, "Home localization bindings missing");
                foreach (var label in home.GetComponentsInChildren<TMP_Text>()) Check(!label.text.Contains("No translation"), "Missing translation");
                for (int i = 1; i <= 4; i++)
                {
                    Click(home.Buttons[i]); yield return null; yield return new WaitForEndOfFrame();
                    var back = Controller.GetComponentsInChildren<UnityEngine.UI.Button>().First(b => b.name == "HomeButton" || b.GetComponentInChildren<TMP_Text>()?.text == "Главная");
                    Click(back); yield return null; yield return new WaitForEndOfFrame(); Check(CurrentPage() == "Home", "Home return failed");
                }
                Click(home.Buttons[5]); yield return null; yield return new WaitForEndOfFrame();
                Check(CurrentPage() == "Gallery" && Field<int>("selectedChapter") == 0, "Continue must open first painting");
                Call("FindArtworkArtifact", 0, 0); Call("UseHintForCurrentChapter"); Call("ClaimDailyBonus");
                Check(Field<int>("hintCount") == 3, "Hint and bonus balance incorrect");
                Check(AirtistProgress.Load().chapters[0].foundMask == 1, "Found artifact not persisted");
            }
            else
            {
                Check(Field<bool[][]>("artifactFound")[0][0], "Artifact lost after Play restart");
                Check(Field<bool[]>("hintUsedForChapter")[0], "Hint state lost after restart");
                Check(Field<int>("hintCount") == 3 && Field<bool>("dailyBonusClaimed"), "Balance or daily state lost");
                Call("ClaimDailyBonus"); Check(Field<int>("hintCount") == 3, "Daily bonus paid twice");
                Call("ContinueAdventure"); Check(CurrentPage() == "Gallery", "Partial painting not resumed");
                Call("FindArtworkArtifact", 0, 1); Call("FindArtworkArtifact", 0, 2); Check(CurrentPage() == "Found", "Completion not shown");
                Show("Home"); Call("ContinueAdventure"); Check(CurrentPage() == "Found", "Pending collection not resumed");
                Call("CollectCurrentChapterAndOpenCollection"); Check(CurrentPage() == "Collection", "Collection not opened");
                Check(Field<bool[]>("chapterCollected")[0], "Collection state not saved");
                yield return null; yield return new WaitForEndOfFrame();
                Click(Field<UnityEngine.UI.Button>("collectionNextButton"));
                Check(CurrentPage() == "Gallery" && Field<int>("selectedChapter") == 1, "Next painting failed");
                for (int i = 0; i < 3; i++) Call("FindArtworkArtifact", 1, i);
                Call("CollectCurrentChapterAndOpenNextChapter"); Check(Field<int>("selectedChapter") == 2, "Third painting failed");
                for (int i = 0; i < 3; i++) Call("FindArtworkArtifact", 2, i);
                Call("CollectCurrentChapterAndOpenCollection"); Call("ContinueAdventure");
                Check(CurrentPage() == "Collection" && Field<bool[]>("chapterCollected").All(v => v), "Completed chapter should open collection");
                Check(AirtistProgress.Load().chapters.All(c => c.collected && c.foundMask == 7), "Final save incomplete");
            }
        }
        private static void Click(UnityEngine.UI.Button button)
        {
            Check(button != null && button.IsInteractable(), "Inactive button");
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject, "Button raycast blocked: " + button.name);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
        public static string SetResolution(int width, int height)
        {
            var assembly = typeof(EditorWindow).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var sizes = typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
            var group = sizesType.GetProperty("currentGroup").GetValue(sizes);
            var names = (string[])group.GetType().GetMethod("GetDisplayTexts").Invoke(group, null);
            string name = "AIrtist QA " + width + "x" + height;
            int index = Array.FindIndex(names, n => n.Contains(name));
            if (index < 0)
            {
                var size = Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"), new object[] { Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"), "FixedResolution"), width, height, name });
                group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
                index = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null) - 1;
            }
            var t = assembly.GetType("UnityEditor.GameView"); var w = EditorWindow.GetWindow(t);
            var f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            t.GetProperty("selectedSizeIndex", f).SetValue(w, index); w.maximized = false; w.Focus(); w.Repaint(); EditorApplication.QueuePlayerLoopUpdate();
            Application.runInBackground = true;
            return name;
        }
        public static void FitGameView()
        {
            var t = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"); var w = EditorWindow.GetWindow(t);
            var f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            t.GetMethod("SnapZoom", f).Invoke(w, new object[] { (float)t.GetProperty("minScale", f).GetValue(w) }); w.Repaint();
        }
        public static string OpenPage(string page) { Show(page); return CurrentPage(); }
        public static string CaptureLayouts()
        {
            LastResult = "Capturing";
            Controller.StartCoroutine(Capture());
            return LastResult;
        }
        private static IEnumerator Capture()
        {
            string dir = Path.GetFullPath("../HomeReview/2026-09-21"); Directory.CreateDirectory(dir);
            var report = new List<string>();
            foreach (var resolution in new[] { new Vector2Int(1920,1080), new Vector2Int(2340,1080), new Vector2Int(1600,1200) })
            {
                SetResolution(resolution.x, resolution.y);
                yield return null; yield return new WaitForEndOfFrame();
                foreach (string page in new[] { "Home", "WorldMap", "Gallery", "Found", "Collection", "Museum", "DailyBonus", "Store", "Profile" })
                {
                    Show(page); yield return null; yield return new WaitForEndOfFrame();
                    foreach (var button in Controller.GetComponentsInChildren<UnityEngine.UI.Button>())
                    {
                        var corners = new Vector3[4]; ((RectTransform)button.transform).GetWorldCorners(corners);
                        if (corners.Any(p => p.x < -1 || p.y < -1 || p.x > Screen.width + 1 || p.y > Screen.height + 1)) report.Add("OUTSIDE " + resolution + " " + page + " " + button.name);
                    }
                    if (page == "Home")
                        foreach (var label in Controller.GetComponentsInChildren<TMP_Text>())
                            if (label.isTextOverflowing) report.Add("TEXT OVERFLOW " + resolution + " " + label.name);
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir, page + "_" + resolution.x + "x" + resolution.y + ".png"));
                    yield return null; yield return new WaitForEndOfFrame();
                }
            }
            SetResolution(1920,1080); Show("Home");
            yield return null; yield return new WaitForEndOfFrame(); FitGameView();
            LastResult = report.Count == 0 ? "PASS layout bounds / Home text: 9 screens x 3 sizes" : string.Join("\n", report);
            File.WriteAllText("Library/AIrtistLayouts.txt", LastResult);
        }
    }
}
