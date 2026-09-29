using System;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistMaxAdProvider : AirtistContinuationAdProvider
    {
        [SerializeField] private string rewardedId="ff2aeb83c4c4b91f";
        [SerializeField] private string interstitialId="2cfef6840c15a3b3";
        private bool initialized, requested, earned;
        private int retries;
        private Action<bool> completion;
        public override bool IsReady => initialized && completion==null && MaxSdk.IsRewardedAdReady(rewardedId);

        // MAX runs its configured UMP flow before OnSdkInitializedEvent.
        // Do not set consent flags here: the player's choices belong to the CMP.
        private void Start()
        {
            InitializeAfterPrivacySetup();
        }

        // Dashboard privacy message and the MAX consent flow must be configured.
        public void InitializeAfterPrivacySetup()
        {
            if(requested || Application.platform!=RuntimePlatform.Android)return;
            requested=true;
            MaxSdkCallbacks.OnSdkInitializedEvent+=Initialized;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent+=Loaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent+=LoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent+=Rewarded;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent+=Hidden;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent+=DisplayFailed;
#if DEVELOPMENT_BUILD
            // Samsung S21 test device. Excluded from release builds.
            MaxSdk.SetTestDeviceAdvertisingIdentifiers(new[] { "0c5872f2-3b28-4594-92dd-04880e607000" });
#endif
            MaxSdk.InitializeSdk();
        }
        private void Initialized(MaxSdkBase.SdkConfiguration configuration)
        {
            if (!configuration.IsSuccessfullyInitialized)
            {
                Debug.LogWarning("[AIrtist Ads] MAX initialization failed. Ads remain unavailable.");
                return;
            }
#if DEVELOPMENT_BUILD
            // Verify the registered device entered test mode. Never preload live ads in test builds.
            MaxSdk.ShowMediationDebugger();
            if (!configuration.IsTestModeEnabled)
            {
                Debug.LogWarning("[AIrtist Ads] Enable Test Ads in MAX diagnostics and restart. Game ad requests are blocked until test mode is active.");
                return;
            }
#endif
            initialized=true;
            Load();
        }
        private void Load(){if(initialized)MaxSdk.LoadRewardedAd(rewardedId);}
        private void Loaded(string id,MaxSdkBase.AdInfo info){if(id==rewardedId){retries=0;CancelInvoke(nameof(Load));}}
        private void LoadFailed(string id,MaxSdkBase.ErrorInfo error)
        {
            if(id!=rewardedId)return;
            CancelInvoke(nameof(Load));Invoke(nameof(Load),Mathf.Pow(2,Mathf.Min(++retries,6)));
        }
        public override void ShowRewarded(Action<bool> completed)
        {
            if(!IsReady){completed?.Invoke(false);return;}
            completion=completed;earned=false;
            try{MaxSdk.ShowRewardedAd(rewardedId);}
            catch{Resolve(false);Load();}
        }
        private void Rewarded(string id,MaxSdkBase.Reward reward,MaxSdkBase.AdInfo info)
        {if(id==rewardedId && completion!=null)earned=true;}
        private void Hidden(string id,MaxSdkBase.AdInfo info)
        {if(id!=rewardedId)return;Resolve(earned);Load();}
        private void DisplayFailed(string id,MaxSdkBase.ErrorInfo error,MaxSdkBase.AdInfo info)
        {if(id!=rewardedId)return;Resolve(false);Load();}
        private void Resolve(bool success)
        {
            var callback=completion;completion=null;earned=false;callback?.Invoke(success);
        }
        private void OnDestroy()
        {
            CancelInvoke();
            MaxSdkCallbacks.OnSdkInitializedEvent-=Initialized;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent-=Loaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent-=LoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent-=Rewarded;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent-=Hidden;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent-=DisplayFailed;
            Resolve(false);
        }
        // Reserved only. No interstitial loading or showing until placement rules are approved.
        public string InterstitialId => interstitialId;
    }
}
