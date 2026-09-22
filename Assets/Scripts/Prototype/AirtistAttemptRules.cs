using System;
using UnityEngine;

namespace Airtist.Prototype
{
    public enum AirtistAttemptMode { Clicks, Time, ClicksAndTime }

    [CreateAssetMenu(menuName = "AIrtist/Attempt rules")]
    public sealed class AirtistAttemptRules : ScriptableObject
    {
        [Serializable]
        public sealed class PaintingRule
        {
            public string chapterId;
            public AirtistAttemptMode mode = AirtistAttemptMode.Clicks;
            [Min(1)] public int clicks = 12;
            [Min(1)] public float seconds = 180;
            [Min(1)] public int continuationClicks = 3;
            [Min(1)] public float continuationSeconds = 30;
            [Tooltip("0: 75% of the time limit, also used for click-only levels.")] public float starSeconds;
            [Tooltip("0: number of hidden objects plus one miss.")] public int starChecks;
        }
        public PaintingRule[] paintings = {
            new PaintingRule { chapterId="mona-lisa", mode=AirtistAttemptMode.Clicks, clicks=12, seconds=180 },
            new PaintingRule { chapterId="liberty", mode=AirtistAttemptMode.Time, clicks=8, seconds=120 },
            new PaintingRule { chapterId="medusa", mode=AirtistAttemptMode.ClicksAndTime, clicks=9, seconds=150 }
        };
        public PaintingRule Get(string id) => Array.Find(paintings ?? Array.Empty<PaintingRule>(), p => p != null && p.chapterId == id)
            ?? new PaintingRule { chapterId=id };
    }

    [Serializable]
    public sealed class AirtistAttemptState
    {
        public string attemptId;
        public AirtistAttemptMode mode;
        public int clicksLeft;
        public float secondsLeft;
        public int continuationClicks;
        public float continuationSeconds;
        public bool rewardedContinuationUsed;
        public int metricsVersion;
        public float elapsedSeconds;
        public int checksUsed;
        public bool UsesClicks => mode != AirtistAttemptMode.Time;
        public bool UsesTime => mode != AirtistAttemptMode.Clicks;
        public bool Exhausted => (UsesClicks && clicksLeft <= 0) || (UsesTime && secondsLeft <= 0);

        public static AirtistAttemptState Start(AirtistAttemptRules.PaintingRule rule) => new AirtistAttemptState {
            attemptId=Guid.NewGuid().ToString("N"), mode=rule.mode, metricsVersion=1,
            clicksLeft=Mathf.Max(1,rule.clicks), secondsLeft=Mathf.Max(1,rule.seconds),
            continuationClicks=Mathf.Max(1,rule.continuationClicks), continuationSeconds=Mathf.Max(1,rule.continuationSeconds)
        };
        public void Tick(float elapsed)
        {
            if(Exhausted) return;
            float active=Mathf.Max(0,elapsed);
            if(UsesTime) active=Mathf.Min(active,secondsLeft);
            elapsedSeconds+=active;
            if(UsesTime) secondsLeft=Mathf.Max(0,secondsLeft-active);
        }
        public bool TryCheck()
        {
            if (Exhausted) return false;
            if (UsesClicks) clicksLeft--;
            checksUsed++;
            return true;
        }
        public bool Continue()
        {
            if (!Exhausted) return false;
            if (UsesClicks && clicksLeft <= 0) clicksLeft=Mathf.Max(1,continuationClicks);
            if (UsesTime && secondsLeft <= 0) secondsLeft=Mathf.Max(1,continuationSeconds);
            return true;
        }
    }
}
