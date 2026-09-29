# Hero selection, button finish and menu click — 29 September 2026

## Implemented

- Three coordinated character cards, a check badge and explicit selection label; selection saves immediately.
- Matching transparent Leo and Mika portraits join the existing Amelie sprite. Home, search, both gallery views and store use the saved choice; gallery profile name also changes.
- Home shows the current hero name in a clear “Мика · сменить героя” button (with the selected name).
- Returning players can confirm a hero directly. Viewing/skipping their story no longer restarts Mona Lisa or the tutorial. New-player story/skip still leads to the original tutorial.
- Existing save `hero` field remains compatible. Invalid indices are clamped.
- Common menu buttons retain the approved palette with a restrained upper sheen, inner rim, lower bevel and short pressed-color transition. Decorations never intercept touches. Disabled controls have subdued highlights.
- `Assets/Audio/UI/UI_Click.mp3` is connected through SettingsConfig at volume 0.55. One controller-owned 2D source survives closing individual panels; the sound setting mutes it. Repeated theme updates do not duplicate callbacks. No click sound is attached to artwork hit targets.
- The supplied MP3 and background music/import settings were not modified.

## Verification

- Isolated edit-mode `AirtistHeroVerification.Verify()` exercises each portrait button, persistence, return, a fresh controller reload, matching portraits in all slots, story replay without progress reset, invalid index handling, click binding and non-blocking decoration.
- Verification writes only a unique temporary PlayerPrefs test key and removes it afterwards. The real profile is checked byte-for-byte unchanged.
- Static previews: wide phone with safe insets and 4:3 tablet, hero picker, home, search, energy and settings. No Play Mode or APK build used.
- Still to check on Samsung: actual click loudness/latency, rapid navigation, mute/unmute and restart with each hero.

## Generated artwork

Built-in image generation, using `Assets/Art/ApprovedUI/Amelie.png` as the style anchor and `Assets/Art/Introduction/CharacterPortraits.png` for Leo/Mika identity and clothing. Existing Amelie and approved comic artwork retained.

Final project assets: `Assets/Art/ApprovedUI/Leo.png`, `Assets/Art/ApprovedUI/Mika.png`.
Imported as transparent sprites, max size 1024, no mipmaps/read-write, Android ASTC 6x6.

### Leo prompt

Use case: style-transfer. Asset type: production transparent character bust sprite for AIrtist mobile museum hidden-object game. Image 1 is the exact rendering style and bust composition reference (Amelie). Image 2 is the approved identity/outfit reference sheet, not a UI to reproduce. Create ONE isolated character, matching image 1's polished warm hand-painted cartoon rendering, friendly expressive large eyes, fine outlines, warm skin shading, fabric detail, and appealing silhouette. Framing: head, shoulders and torso to forearms, complete hair and elbows, centered, comfortable transparent padding, subject fills most height, no frame, no background, no letters, no UI, no watermark. Genuinely transparent alpha background. Subject: LEO, the middle male character in reference 2. Preserve his wavy tousled chestnut hair, hazel eyes, open sage-olive overshirt over cream T-shirt and dark sketchbook held comfortably in his arms. Young adult art restoration student, clever gentle smile. Looking slightly toward the viewer's left. Do NOT include Amelie or Mika. Do not copy photographic style from image2; use cartoon style of image1.

### Mika prompt

Use case: style-transfer. Asset type: production transparent character bust sprite for AIrtist mobile museum hidden-object game. Image 1 is the exact rendering style and bust composition reference (Amelie). Image 2 is the approved identity/outfit reference sheet, not a UI to reproduce. Create ONE isolated character, matching image 1's polished warm hand-painted cartoon rendering, friendly expressive large eyes, fine outlines, warm skin shading, fabric detail, and appealing silhouette. Framing: head, shoulders and torso to forearms, complete hair and elbows, centered, comfortable transparent padding, subject fills most height, no frame, no background, no letters, no UI, no watermark. Genuinely transparent alpha background. Subject: MIKA, the right character in reference 2. Preserve chin-length softly wavy deep blue hair, thin round gold spectacles worn on face, intelligent feminine/androgynous appearance, lavender cardigan over ivory collared blouse and books held comfortably at chest. Young adult art history student, warm curious smile. Looking slightly toward the viewer's left. Do NOT include Amelie or Leo. Do not copy photographic style from image2; use cartoon style of image1.
