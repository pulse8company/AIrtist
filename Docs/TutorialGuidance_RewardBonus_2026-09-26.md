# Tutorial guidance and video bonus

## Implemented
- Golden, non-interactive outline follows the relevant tool. It does not consume taps.
- Tutorial targets (moustache and the activated exact-hint watch) receive a frame and a contrasting “Здесь” label. No guidance reveals the fourth brooch.
- OK remains available on every step. Skipping does not award a finding.
- Video now grants random hint inventory, not an automatic exact target: uniformly 1–3 hints by default. Inspector fields `videoBonusMinimum` and `videoBonusMaximum` control the range (validated to 1–100).
- Grant occurs only after the existing provider confirms `earned=true`; a per-request guard ignores duplicate callbacks. Cancellation/failure gives nothing. Grant is saved immediately, works independently of painting selection, and does not count as using a hint.
- A six-second, non-blocking notification shows the result. Existing exact hints are not overwritten.

## Validation and limitations
Scripts compiled successfully. An isolated 1600×720 tutorial UI preview was inspected. No Play, gameplay tests, APK builds, ads, or player-save resets were run.

The project currently exposes an abstract `AirtistContinuationAdProvider`, with no supplied SDK adapter. This change implements reward-side logic only; real video playback is not enabled or validated. The ordinary video button stays unavailable without a ready provider. Tutorial video remains explanation-only.

## User phone checks
1. Every tutorial step highlights the matching tool; outlined tools and objects still accept taps.
2. Check → moustache, Exact → watch; marks/zoom/OK advance without getting stuck.
3. Guidance disappears after training and when leaving the search screen; brooch remains unmarked.
4. Once an ad adapter is connected: complete/cancel/fail a video, verify only completion grants 1–3 hints and reports the correct amount.
5. Verify reward survives restart, duplicate provider callbacks do not double-grant, and an existing exact hint remains intact.
