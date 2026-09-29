# MAX Android integration — handoff

## Installed / configured
- Official MAX Unity plugin 8.6.6 (Android SDK 13.6.4), imported from AppLovin's GitHub release.
- Official MAX Google AdMob adapter Android 25.5.0.0 / iOS 13.10.0.0 imported from the Integration Manager feed.
- SDK key stored in AppLovinSettings, not repeated in this document. AdMob Android App ID configured.
- AirtistMaxAdProvider attached to the active scene controller's continuationAds reference.
- Rewarded MAX unit: ff2aeb83c4c4b91f. Interstitial ID reserved: 2cfef6840c15a3b3. No interstitial loading/showing implemented or enabled yet.
- Rewarded adapter records the earned event and resolves once after close; a close without reward or display failure resolves false. Load retries use exponential delays up to 64 seconds. Existing game callbacks award energy, hints or continuation.

## Not yet ready for production / phone ad test
- Initialization is deliberately not automatic. InitializeAfterPrivacySetup must be wired to a verified consent/CMP flow. No consent is assumed or forced true.
- Supplied privacy URL: https://www.pulse8gaming.com/privacy-policy . Its introductory game list does not yet mention AIrtist. Owner should review applicability and provide Terms URL if used.
- Configure Google consent messages and mediation in the dashboards. AdMob rewarded  ca-app-pub-7226446414633998/7265454298 and interstitial ca-app-pub-7226446414633998/5535698946 belong in the MAX network mapping, not MaxSdk.Show calls. Dashboard changes were not made.
- Resolve Android native dependencies and inspect the Mediation Debugger on a registered test device before live traffic. No test device identifier has been supplied.
- Existing LevelPlay package and its dependency XMLs remain installed. Review/remove obsolete LevelPlay integration in a separate migration step before final Android validation; no destructive cleanup was performed.
- No APK, Play, real ad request, gameplay test or payment was run.

## Verification
The new provider compiled successfully against the installed MAX plugin. Actual network loading, consent flow, close/reward ordering and Android dependency compatibility remain device checks, not verified results.
