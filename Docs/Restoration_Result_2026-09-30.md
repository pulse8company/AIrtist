# Painting completion — 30 September 2026

## Implemented

- Finished result layout in the existing museum / cream / wooden-frame style. Painting name now sits on a readable plaque.
- Bold congratulations heading and three separate star cards: all details, time, accuracy. Each card has its measured value and target; old saves show “Нет оценки” rather than fabricated metrics.
- Coin and energy reward tiles use existing painted icons and show the amounts recorded by the economy. UI changes never grant rewards.
- Clear note that rewards are already credited. Zero-reward replays distinguish remaining star rewards from a painting whose rewards are all paid.
- Styled gallery / next-painting / improve-result actions; when every painting is complete, the next action hides and the gallery action expands.
- All new decorations ignore pointer hits. Existing input areas remain intact.

## Sound

The supplied `Assets/Audio/UI/UI_PaintingComplete.mp3` (2.568 seconds) is assigned to `SettingsConfig.paintingComplete`, volume 0.65. The MP3 and its import settings are unchanged.

`AirtistCompletionCue` arms only when a new attempt finishes, then consumes once when the result page is shown. Reopening a collected painting, refreshing labels, or reopening the result does not rearm it. A newly completed replay has its own attempt id and may celebrate again. Muted results consume the cue without playing, so unmuting later does not replay an old completion.

Audio uses a separate controller-owned 2D source, no loop, and the existing sound preference / AudioListener volume. It does not change background-music volume or reward calculations. Edit-mode previews never play audio.

## Verification

- C# compilation succeeds.
- `AirtistRestorationVerification.Verify()` covers cue idempotency, normal and exact-brush final finds, result navigation, no duplicate reward on reopen, 1/2/3 star states, non-blocking graphics, clickable buttons, and the final painting with no next content.
- Tests run on disposable preview-scene copies. No Play Mode, live save writes, ads, purchases, or APK build. Real progress is compared byte-for-byte before/after.
- Static previews: wide landscape safe-area phone, 4:3 tablet, 1/2/3 stars, replay without new rewards, old-save unmeasured result. Preview coin/energy values are synthetic examples, not a new economy configuration.

## Phone checks still required

1. Finish a painting normally: one sound and correct stars/rewards.
2. Reopen its result from the gallery: no new sound or reward.
3. Turn sound off, finish another painting: silent. Turn on afterwards: no deferred celebration.
4. Complete a new replay or use the exact/remove brush for the last object: one celebration.
5. Check loudness against music and screen fit on the Samsung S21.
