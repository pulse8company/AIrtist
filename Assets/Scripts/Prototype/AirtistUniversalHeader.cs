using System;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        private RectTransform universalHeader;
        private GameObject universalClose;
        private GameObject universalHome;
        private TMPro.TextMeshProUGUI headerEnergy, headerCoins;

        private void BuildUniversalHeader()
        {
            var approved=AirtistApprovedTheme.Current;
            if(approved!=null){BuildApprovedHeader(approved);return;}
            if (homeScreenPrefab == null) return;
            universalHeader = new GameObject("UniversalHomeHeader", typeof(RectTransform), typeof(UnityEngine.UI.AspectRatioFitter)).GetComponent<RectTransform>();
            universalHeader.SetParent(transform, false);
            universalHeader.anchorMin = universalHeader.anchorMax = Vector2.one * .5f;
            universalHeader.sizeDelta = new Vector2(1672, 941);
            var fit = universalHeader.GetComponent<UnityEngine.UI.AspectRatioFitter>();
            fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
            fit.aspectRatio = 1672f / 941;
            var clip = new GameObject("HeaderClip", typeof(RectTransform), typeof(UnityEngine.UI.RectMask2D)).GetComponent<RectTransform>();
            clip.SetParent(universalHeader, false);
            clip.anchorMin = new Vector2(0, 831f / 941); clip.anchorMax = Vector2.one;
            clip.offsetMin = clip.offsetMax = Vector2.zero;
            string[] names = { "GiftButton", "MapButton", "CollectionButton", "StoreButton", "ProfileButton" };
            Action[] actions = { OpenDailyBonus, () => Show(Page.WorldMap), () => Show(Page.Collection), () => Show(Page.Store), () => Show(Page.Profile) };
            foreach (RectTransform source in homeScreenPrefab.transform)
            {
                if (source.anchorMax.y <= 831f / 941) continue;
                if (source.name == "Text_home.location") continue;
                if (source.name == "CollectionButton" || source.name == "ProfileButton" || source.name == "StoreButton"
                    || source.name == "Text_nav.collection" || source.name == "Text_nav.profile" || source.name == "Text_nav.store") continue;
                var copy = Instantiate(source, clip, false);
                copy.name = source.name;
                copy.anchorMin = new Vector2(source.anchorMin.x, (source.anchorMin.y * 941 - 831) / 110);
                copy.anchorMax = new Vector2(source.anchorMax.x, (source.anchorMax.y * 941 - 831) / 110);
                var scaled = copy.GetComponent<AirtistScaledLabel>();
                if (scaled != null) scaled.artboard = universalHeader;
                var button = copy.GetComponent<UnityEngine.UI.Button>();
                int index = Array.IndexOf(names, source.name);
                if (button != null && index >= 0)
                {
                    button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    var action = actions[index]; button.onClick.AddListener(() => action());
                }
            }
            // Cover the old navigation artwork beneath the shared header on Home too.
            var menuBacking = CreatePanel(clip, "MergedNavigationBacking", Parchment, Anchor.Stretch, Vector2.zero, Vector2.zero);
            menuBacking.anchorMin = new Vector2(1138f / 1672, 0); menuBacking.anchorMax = new Vector2(1590f / 1672, 1);
            menuBacking.offsetMin = menuBacking.offsetMax = Vector2.zero;
            menuBacking.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            CreateGalleryNavigationButton(clip, "Моя галерея", 1144, 202, homeScreenPrefab.Buttons[2], () => Show(Page.Collection));
            CreateGalleryNavigationButton(clip, "Магазин", 1360, 139, homeScreenPrefab.Buttons[3], () => Show(Page.Store));
            var home = CreateButton(clip, "Home", Color.clear, DeepTeal, Anchor.TopLeft,
                Vector2.zero, new Vector2(139, 87), () => Show(Page.Home), 24);
            home.name = "HomeButton";
            var homeRect = (RectTransform)home.transform;
            homeRect.anchorMin = new Vector2(718f / 1672, 16f / 110);
            homeRect.anchorMax = new Vector2(857f / 1672, 103f / 110);
            homeRect.offsetMin = homeRect.offsetMax = Vector2.zero;
            home.GetComponent<UnityEngine.UI.Image>().raycastPadding = Vector4.zero;
            ApplyHomeButtonSurface(home);
            var homeLabel = GetButtonLabel(home);
            homeLabel.rectTransform.anchorMin = Vector2.zero;
            homeLabel.rectTransform.anchorMax = Vector2.one;
            homeLabel.rectTransform.offsetMin = new Vector2(8, 4);
            homeLabel.rectTransform.offsetMax = new Vector2(-8, -4);
            homeLabel.transform.SetAsLastSibling();
            var homeLabelScale = homeLabel.gameObject.AddComponent<AirtistScaledLabel>();
            homeLabelScale.artboard = universalHeader;
            homeLabelScale.designFontSize = 24;
            universalHome = home.gameObject;
            universalHome.SetActive(false);
            // A plain close icon, always Home; never browser-style back navigation.
            var close = CreateButton(clip, "×", Color.clear, DeepTeal, Anchor.TopRight, Vector2.zero, new Vector2(70, 80), () => Show(Page.Home), 40);
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = new Vector2(1513f / 1672, 16f / 110);
            closeRect.anchorMax = new Vector2(1583f / 1672, 103f / 110);
            closeRect.offsetMin = closeRect.offsetMax = Vector2.zero;
            var closeLabel = GetButtonLabel(close);
            closeLabel.rectTransform.anchorMin = Vector2.zero; closeLabel.rectTransform.anchorMax = Vector2.one;
            closeLabel.rectTransform.offsetMin = closeLabel.rectTransform.offsetMax = Vector2.zero;
            close.GetComponent<UnityEngine.UI.Image>().raycastPadding = Vector4.zero;
            universalClose = close.gameObject;
            BuildHeaderWallet(clip);
        }

        private void BuildHeaderWallet(RectTransform clip)
        {
            // Cover the old location artwork, including the pin, on Home and other screens.
            var backing=CreatePanel(clip,"WalletBacking",Parchment,Anchor.Stretch,Vector2.zero,Vector2.zero);
            WalletRect(backing,374,5,334,98,1672,110);
            backing.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            headerEnergy=CreateWalletBadge(clip,"Energy","Энергия",380,"EnergyIcon");
            headerCoins=CreateWalletBadge(clip,"Coins","Монеты",546,"CoinsIcon");
            if(attemptHud!=null)
            {
                attemptHud.energyLabel.gameObject.SetActive(false);
                attemptHud.coinsLabel.gameObject.SetActive(false);
                foreach(string name in new[]{"EnergyIcon","CoinsIcon","EnergyCaption","CoinsCaption","StatusDividerHorizontal"})
                {
                    var child=gameplayScreen.transform.Find(name);
                    if(child!=null) child.gameObject.SetActive(false);
                }
                var plaque=gameplayScreen.transform.Find("AttemptStatusPanel") as RectTransform;
                if(plaque!=null) WalletRect(plaque,8,117,320,105,1672,941);
                var divider=gameplayScreen.transform.Find("StatusDividerVertical") as RectTransform;
                if(divider!=null) WalletRect(divider,170,132,1.3f,75,1672,941);
            }
            RefreshHeaderWallet();
        }

        private TMPro.TextMeshProUGUI CreateWalletBadge(RectTransform parent,string name,string caption,float x,string iconName)
        {
            var badge=CreatePanel(parent,name+"Wallet",Color.white,Anchor.TopLeft,Vector2.zero,Vector2.zero);
            WalletRect(badge,x,15,156,78,1672,110);
            var image=badge.GetComponent<UnityEngine.UI.Image>();
            image.sprite=galleryMenuPanel!=null?galleryMenuPanel:roundedSprite;
            image.type=UnityEngine.UI.Image.Type.Sliced; image.pixelsPerUnitMultiplier=6; image.raycastTarget=false;
            var source=gameplayScreen!=null?gameplayScreen.transform.Find(iconName):null;
            if(source!=null)
            {
                var icon=Instantiate(source,badge,false) as RectTransform;
                icon.gameObject.SetActive(true); WalletRect(icon,10,24,32,42,156,78);
            }
            var title=CreateLabel(badge,caption,16,DeepTeal,Anchor.Center,Vector2.zero,Vector2.zero,TMPro.TextAlignmentOptions.Center);
            WalletRect(title.rectTransform,43,10,104,23,156,78);
            var value=CreateLabel(badge,"0",24,DeepTeal,Anchor.Center,Vector2.zero,Vector2.zero,TMPro.TextAlignmentOptions.Center,TMPro.FontStyles.Bold);
            WalletRect(value.rectTransform,43,33,104,35,156,78);
            foreach(var label in new[]{title,value})
            {
                var scale=label.gameObject.AddComponent<AirtistScaledLabel>();
                scale.artboard=universalHeader; scale.designFontSize=label==title?16:24;
                label.overflowMode=TMPro.TextOverflowModes.Ellipsis;
            }
            return value;
        }

        private static void WalletRect(RectTransform rect,float x,float y,float w,float h,float pw,float ph)
        {
            rect.anchorMin=new Vector2(x/pw,1-(y+h)/ph); rect.anchorMax=new Vector2((x+w)/pw,1-y/ph);
            rect.offsetMin=rect.offsetMax=Vector2.zero;
        }

        private void RefreshHeaderWallet()
        {
            if(headerEnergy!=null) headerEnergy.text=energy+"/"+EnergyCap;
            if(headerCoins!=null) headerCoins.text=coins.ToString();
        }

        private void StyleMapZoomButtons(AirtistWorldMapScreen map)
        {
            if (homeScreenPrefab == null) return;
            for (int i = 3; i < 5; i++)
            {
                var button = map.Controls[i];
                ApplyHomeButtonSurface(button);
                CreateLabel((RectTransform)button.transform, i == 3 ? "−" : "+", 40, DeepTeal,
                    Anchor.Stretch, Vector2.zero, Vector2.zero, TMPro.TextAlignmentOptions.Center);
            }
        }

        private void CreateGalleryNavigationButton(RectTransform parent, string caption, float x, float width,
            UnityEngine.UI.Button source, Action click)
        {
            var button = CreateButton(parent, caption, Color.white, DeepTeal, Anchor.TopLeft,
                Vector2.zero, new Vector2(width, 87), click, 19);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(x / 1672, 16f / 110);
            rect.anchorMax = new Vector2((x + width) / 1672, 103f / 110);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = button.GetComponent<UnityEngine.UI.Image>();
            ApplyHomeButtonSurface(button);
            image.raycastPadding = Vector4.zero;
            // Icons use a square viewport independently of the button width.
            var iconSlot = new GameObject("IconSlot", typeof(RectTransform)).GetComponent<RectTransform>();
            iconSlot.SetParent(rect, false);
            iconSlot.anchorMin = new Vector2(.25f, .43f); iconSlot.anchorMax = new Vector2(.75f, .90f);
            iconSlot.offsetMin = iconSlot.offsetMax = Vector2.zero;
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(UnityEngine.UI.RawImage), typeof(UnityEngine.UI.AspectRatioFitter));
            icon.transform.SetParent(iconSlot, false);
            var iconFit = icon.GetComponent<UnityEngine.UI.AspectRatioFitter>();
            iconFit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; iconFit.aspectRatio = 1;
            var sourceSprite = source.image.sprite;
            var sourceRect = sourceSprite.rect;
            var iconImage = icon.GetComponent<UnityEngine.UI.RawImage>();
            iconImage.texture = sourceSprite.texture;
            iconImage.uvRect = new Rect((sourceRect.center.x - 22) / sourceSprite.texture.width,
                (sourceRect.yMax - 53) / sourceSprite.texture.height, 44f / sourceSprite.texture.width, 44f / sourceSprite.texture.height);
            iconImage.raycastTarget = false;
            var label = GetButtonLabel(button);
            label.rectTransform.anchorMin = new Vector2(.06f, .07f);
            label.rectTransform.anchorMax = new Vector2(.94f, .40f);
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            var scaled = label.gameObject.AddComponent<AirtistScaledLabel>();
            scaled.artboard = universalHeader; scaled.designFontSize = 19;
            label.transform.SetAsLastSibling();
        }

        private void ApplyHomeButtonSurface(UnityEngine.UI.Button button)
        {
            if (galleryMenuPanel != null)
            {
                var surface = button.GetComponent<UnityEngine.UI.Image>();
                surface.sprite = galleryMenuPanel;
                surface.type = UnityEngine.UI.Image.Type.Sliced;
                surface.pixelsPerUnitMultiplier = 6;
                surface.color = Color.white;
                button.targetGraphic = surface;
                return;
            }
            var reference = homeScreenPrefab.Buttons[1].GetComponent<UnityEngine.UI.Image>().sprite;
            // Reuse the clean lower half of Home's cream/gold button, mirrored for the top.
            // UV-only UI composition: original texture and imported sprites are unchanged.
            Rect r = reference.rect;
            Rect uv = new Rect(r.x / reference.texture.width, r.y / reference.texture.height,
                r.width / reference.texture.width, r.height * .40f / reference.texture.height);
                button.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
                for (int half = 0; half < 2; half++)
                {
                    var go = new GameObject("HomeButtonSurface", typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
                    var rect = go.GetComponent<RectTransform>(); rect.SetParent(button.transform, false);
                    rect.anchorMin = new Vector2(0, half * .5f); rect.anchorMax = new Vector2(1, (half + 1) * .5f);
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                    var image = go.GetComponent<UnityEngine.UI.RawImage>(); image.texture = reference.texture;
                    image.uvRect = half == 0 ? uv : new Rect(uv.x, uv.yMax, uv.width, -uv.height);
                    image.raycastTarget = false;
                    if (half == 0) button.targetGraphic = image;
                }
        }
    }
}
