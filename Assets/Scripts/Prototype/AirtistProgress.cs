using System;
using UnityEngine;

namespace Airtist.Prototype
{
    [Serializable]
    public sealed class AirtistProgress
    {
        public const string SaveKey = "AIrtist.Progress.v1";
        private static string StorageKey
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.SessionState.GetString("AIrtist.Progress.TestKey", SaveKey);
#else
                return SaveKey;
#endif
            }
        }
        public int version = 1;
        public int contentVersion;
        public int selectedChapter;
        public int hints = 3;
        public string dailyUtc = "";
        public bool rewardedHintClaimed;
        public int economyVersion;
        public int energy = 100;
        public int coins;
        public long energyUpdatedUtc;
        public int continuationBonuses;
        public long energyAdDay;
        public int energyAdsToday;
        public ChapterProgress[] chapters = Array.Empty<ChapterProgress>();

        [Serializable]
        public sealed class ChapterProgress
        {
            public string id;
            public int foundMask;
            public bool collected;
            public bool hintUsed;
            public AirtistAttemptState attempt;
            public bool economyRewardGranted;
            public bool replaying;
            public AirtistRestorationRecord restoration;
            public int areaHintTarget, exactHintTarget;
        }

        public static AirtistProgress Load()
        {
            if (!PlayerPrefs.HasKey(StorageKey)) return new AirtistProgress();
            try
            {
                var state = JsonUtility.FromJson<AirtistProgress>(PlayerPrefs.GetString(StorageKey));
                if (state != null && state.version == 1 && state.chapters != null)
                    return state;
            }
            catch (ArgumentException) { }
            Debug.LogWarning("AIrtist: unreadable progress; using a new local session. Original saved as recovery copy.");
            PlayerPrefs.SetString(StorageKey + ".recovery", PlayerPrefs.GetString(StorageKey));
            PlayerPrefs.Save();
            return new AirtistProgress();
        }

        public void Save()
        {
            PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static void ResetForDevelopment()
        {
            if(PlayerPrefs.HasKey(StorageKey))
                PlayerPrefs.SetString(StorageKey+".beforeDeveloperReset",PlayerPrefs.GetString(StorageKey));
            new AirtistProgress().Save();
        }
#endif
    }
}
