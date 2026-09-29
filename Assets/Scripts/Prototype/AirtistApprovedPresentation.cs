using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        private void BuildApprovedHeader(AirtistApprovedTheme theme)
        {
            universalHeader=new GameObject("UniversalHomeHeader",typeof(RectTransform)).GetComponent<RectTransform>();
            universalHeader.SetParent(transform,false);Stretch(universalHeader);
            var bar=theme.Picture(universalHeader,"HeaderPaper",theme.surface,0,0,1,.105f,false);
            theme.Surface(bar,AirtistApprovedTheme.Paper);theme.Wood(bar);bar.raycastTarget=true;
            Label("Brand","AIrtist",.020f,.018f,.110f,.065f,48);
            theme.Picture(universalHeader,"EnergyIcon",theme.Icon(8),.139f,.024f,.022f,.052f);
            headerEnergy=Label("Energy","",.164f,.028f,.057f,.044f,26);
            theme.Picture(universalHeader,"CoinIcon",theme.Icon(9),.247f,.024f,.024f,.052f);
            headerCoins=Label("Coins","",.277f,.028f,.061f,.044f,28);
            Nav("Gift","",7,.393f,.054f,OpenDailyBonus);
            Nav("Map","Карта",10,.455f,.094f,()=>Show(Page.WorldMap));
            Nav("Collection","Моя галерея",11,.557f,.140f,()=>Show(Page.Collection));
            Nav("Store","Магазин",12,.705f,.105f,()=>Show(Page.Store));
            // Entire visible tiles receive taps, including Home and Close.
            universalHome=Nav("Home","",13,.818f,.052f,()=>Show(Page.Home)).gameObject;
            settingsHeaderButton=Nav("Settings","",-1,.878f,.052f,ToggleSettings);
            theme.Picture(settingsHeaderButton.transform,"Icon",theme.settingsGear,.04f,.04f,.92f,.92f);
            universalClose=Nav("Close","×",-1,.938f,.052f,CloseTopWindow).gameObject;
            theme.Button(universalClose.GetComponent<UnityEngine.UI.Button>(),AirtistApprovedTheme.Coral);
            RefreshHeaderWallet();
            TextMeshProUGUI Label(string name,string text,float x,float y,float w,float h,float size)
            {var t=CreateLabel(universalHeader,text,(int)size,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,TextAlignmentOptions.Left);t.name=name;AirtistApprovedTheme.Rect(t.rectTransform,x,y,w,h);theme.Typography(t,universalHeader,size);return t;}
            UnityEngine.UI.Button Nav(string name,string text,int icon,float x,float w,Action action)
            {
                var b=CreateButton(universalHeader,text,AirtistApprovedTheme.Paper,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,action,22);b.name=name;
                AirtistApprovedTheme.Rect((RectTransform)b.transform,x,.008f,w,.088f);theme.Button(b,AirtistApprovedTheme.Paper);
                var t=GetButtonLabel(b);theme.Typography(t,universalHeader,text=="×"?38:21);
                AirtistApprovedTheme.Rect(t.rectTransform,.05f,.1f,.9f,.8f);
                if(icon>=0)theme.Picture(b.transform,"Icon",theme.Icon(icon),.055f,.08f,string.IsNullOrEmpty(text)?.89f:.27f,.84f);
                if(!string.IsNullOrEmpty(text) && icon>=0)AirtistApprovedTheme.Rect(t.rectTransform,.33f,.1f,.63f,.80f);
                return b;
            }
        }
        private void ApplyApprovedPresentation()
        {
            var theme=AirtistApprovedTheme.Current;if(theme==null)return;
            if(fullBleedPaper!=null && universalHeader!=null)
            {
                var bleed=theme.Picture(universalHeader,"HeaderSafeAreaBleed",theme.surface,0,0,1,.105f,false);
                theme.Surface(bleed,AirtistApprovedTheme.Paper);
                theme.Wood(bleed);bleed.transform.SetAsFirstSibling();
                var follow=bleed.GetComponent<AirtistHeaderBleed>()??bleed.gameObject.AddComponent<AirtistHeaderBleed>();
                follow.source=universalHeader.Find("HeaderPaper") as RectTransform;
                if(follow.source!=null)
                {
                    follow.source.GetComponent<UnityEngine.UI.Image>().color=Color.clear;
                    AirtistApprovedTheme.Hide(follow.source,"OakFrame");
                    AirtistApprovedTheme.Hide(follow.source,"SlimWoodFrame");
                }
            }
            foreach(var pair in pages)
            {
                theme.Common(pair.Value.transform);
                if(pair.Key==Page.WorldMap){pair.Value.GetComponent<UnityEngine.UI.Image>().color=new Color(.68f,.48f,.28f);continue;}
                if(pair.Key==Page.Home)continue;
                var bg=theme.Picture(pair.Value.transform,"ApprovedMuseumBackdrop",theme.museumBackdrop,0,0,1,1,false);bg.transform.SetAsFirstSibling();
                // A single full-screen image keeps its composition across notch/safe-area margins.
                if(fullBleedHome!=null){bg.gameObject.SetActive(false);pair.Value.GetComponent<UnityEngine.UI.Image>().color=Color.clear;}
                var fit=bg.GetComponent<UnityEngine.UI.AspectRatioFitter>()??bg.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=theme.museumBackdrop.rect.width/theme.museumBackdrop.rect.height;
            }
            if(fullBleedHome!=null)
            {var image=fullBleedHome.GetComponent<UnityEngine.UI.Image>();image.sprite=theme.homeBackdrop;fullBleedHome.GetComponent<UnityEngine.UI.AspectRatioFitter>().aspectRatio=theme.homeBackdrop.rect.width/theme.homeBackdrop.rect.height;}
            if(illustratedHome!=null)
            {
                theme.Home(illustratedHome);
                var thumb=illustratedHome.transform.Find("ApprovedRouteThumbnail")?.GetComponent<UnityEngine.UI.Image>();if(thumb!=null)thumb.sprite=MuseumArt(0);
            }
            if(gameplayScreen!=null)
            {
                theme.Gameplay(gameplayScreen);
                var museum=gameplayScreen.viewport.Find("MuseumRoomBackdrop");if(museum!=null)museum.gameObject.SetActive(false);
                gameplayScreen.viewport.GetComponent<UnityEngine.UI.Image>().color=Color.clear;
                foreach(string n in new[]{"EnergyIcon","EnergyCaption","EnergyBalance","CoinsIcon","CoinsCaption","CoinBalance"})AirtistApprovedTheme.Hide(gameplayScreen.transform,n);
            }
            foreach(var gallery in new[]{myGallery,museumGallery})if(gallery!=null)theme.Gallery(gallery);
            foreach(var card in GetComponentsInChildren<AirtistGalleryCard>(true))theme.Card(card);
            foreach(var store in GetComponentsInChildren<AirtistStoreScreen>(true))theme.Store(store);
            foreach(var map in GetComponentsInChildren<AirtistWorldMapScreen>(true))
            {
                if(theme.woodenMap!=null)map.MapContent.GetComponent<UnityEngine.UI.Image>().sprite=theme.woodenMap;
                for(int i=3;i<5;i++){theme.Button(map.Controls[i],AirtistApprovedTheme.Paper);AirtistApprovedTheme.Rect((RectTransform)map.Controls[i].transform,.79f+(i-3)*.065f,.88f,.060f,.10f);}
            }
            ApplyApprovedMuseum(theme);
            ApplyApprovedResult(theme);
            ApplyApprovedGift(theme);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(developerResetRoot!=null)
                foreach(var image in developerResetRoot.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                    if(image.name=="Dialog")theme.Surface(image,AirtistApprovedTheme.Paper);
#endif
            RefreshToolSelection();
        }
        private void ApplyApprovedMuseum(AirtistApprovedTheme theme)
        {
            if(!pages.TryGetValue(Page.MuseumPreview,out var pageObject))return;
            var page=(RectTransform)pageObject.transform;
            foreach(string n in new[]{"MuseumAmbience","ParchmentWash"})AirtistApprovedTheme.Hide(page,n);
            var card=page.Find("MuseumPassport") as RectTransform;
            if(card==null)card=page.Find("MuseumPassportGold") as RectTransform;
            if(card==null)return;
            AirtistApprovedTheme.Rect(card,.115f,.145f,.775f,.805f);theme.Surface(card.GetComponent<UnityEngine.UI.Image>(),AirtistApprovedTheme.Paper);
            theme.Wood(card.GetComponent<UnityEngine.UI.Image>());
            var picture=museumHero.transform.parent as RectTransform;
            AirtistApprovedTheme.Rect(picture,.03f,.04f,.55f,.90f);
            theme.Surface(picture.GetComponent<UnityEngine.UI.Image>(),AirtistApprovedTheme.Paper);
            AirtistApprovedTheme.Rect(museumHero.rectTransform,.035f,.035f,.93f,.93f);
            Place(museumName,.61f,.14f,.35f,.11f,52);Place(museumCity,.61f,.265f,.35f,.06f,26);Place(museumStatus,.61f,.36f,.35f,.07f,28);
            var fact=museumFact.transform.parent as RectTransform;AirtistApprovedTheme.Rect(fact,.60f,.49f,.36f,.27f);theme.Surface(fact.GetComponent<UnityEngine.UI.Image>(),AirtistApprovedTheme.Paper);
            theme.Typography(museumFact,page,23);
            AirtistApprovedTheme.Rect(museumFact.rectTransform,.06f,.28f,.88f,.65f);
            foreach(var b in card.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                var oldFrame=b.transform.parent;b.transform.SetParent(card,false);
                if(oldFrame!=card)oldFrame.gameObject.SetActive(false);
            }
            AirtistApprovedTheme.Rect((RectTransform)museumEnter.transform,.69f,.825f,.27f,.12f);theme.Button(museumEnter,AirtistApprovedTheme.Coral);
            foreach(var b in card.GetComponentsInChildren<UnityEngine.UI.Button>(true))if(b!=museumEnter){AirtistApprovedTheme.Rect((RectTransform)b.transform,.44f,.825f,.23f,.12f);theme.Button(b,AirtistApprovedTheme.Sage);}
            foreach(var b in card.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                foreach(var image in b.GetComponentsInChildren<UnityEngine.UI.Image>(true))if(image!=b.image)image.gameObject.SetActive(false);
                var label=GetButtonLabel(b);AirtistApprovedTheme.Rect(label.rectTransform,.04f,.08f,.92f,.84f);theme.Typography(label,page,28);label.alignment=TextAlignmentOptions.Center;
            }
            void Place(TMP_Text t,float x,float y,float w,float h,float size){AirtistApprovedTheme.Rect(t.rectTransform,x,y,w,h);theme.Typography(t,page,size);}
        }
        private void ApplyApprovedResult(AirtistApprovedTheme theme)
        {
            if(!pages.TryGetValue(Page.Found,out var pageObject))return;
            var page=(RectTransform)pageObject.transform;
            var paper=page.Find("ResultPaper")?.GetComponent<UnityEngine.UI.Image>();theme.Surface(paper,AirtistApprovedTheme.Paper);
            if(paper!=null){AirtistApprovedTheme.Rect(paper.rectTransform,.445f,.115f,.54f,.855f);theme.Wood(paper);}
            AirtistApprovedTheme.Rect(resultPainting.rectTransform,.055f,.12f,.34f,.66f);
            var nameplate=theme.Picture(page,"PaintingNameplate",theme.surface,.055f,.80f,.34f,.072f,false);theme.Surface(nameplate,AirtistApprovedTheme.Paper);
            nameplate.transform.SetSiblingIndex(resultPaintingTitle.transform.GetSiblingIndex());
            Place(resultPaintingTitle,.066f,.811f,.318f,.05f,27,true);
            Place(resultHeading,.477f,.148f,.475f,.072f,38,true);
            Place(resultSubtitle,.475f,.23f,.48f,.038f,21);
            for(int i=0;i<3;i++)
            {
                float x=.477f+i*.162f;
                var mat=theme.Picture(page,"CriterionPaper"+i,theme.surface,x,.29f,.15f,.23f,false);
                theme.Surface(mat,Color.Lerp(AirtistApprovedTheme.Paper,AirtistApprovedTheme.Honey,.10f));
                mat.transform.SetSiblingIndex(resultStars[i].transform.GetSiblingIndex());
                AirtistApprovedTheme.Rect(resultStars[i].rectTransform,x+.022f,.295f,.106f,.14f);
                Place(resultCriteria[i],x+.005f,.438f,.14f,.035f,22,true);
                Place(resultCriterionValues[i],x+.005f,.477f,.14f,.035f,21,true);
            }
            Place(resultMetrics,.478f,.534f,.476f,.042f,19);
            Place(resultRewards,.49f,.589f,.45f,.038f,24,true);
            RewardTile("CoinReward",theme.Icon(9),resultCoins,.484f,AirtistApprovedTheme.Honey);
            RewardTile("EnergyReward",theme.Icon(8),resultEnergy,.725f,AirtistApprovedTheme.Sage);
            Place(resultRewardNote,.48f,.717f,.48f,.035f,19);
            Place(resultFact,.486f,.764f,.467f,.073f,20);
            AirtistApprovedTheme.Rect((RectTransform)resultCollection.transform,.482f,.86f,.205f,.09f);theme.Button(resultCollection,AirtistApprovedTheme.Sage);
            AirtistApprovedTheme.Rect((RectTransform)resultNext.transform,.701f,.86f,.26f,.09f);theme.Button(resultNext,AirtistApprovedTheme.Coral);
            AirtistApprovedTheme.Rect((RectTransform)resultReplay.transform,.055f,.896f,.34f,.072f);theme.Button(resultReplay,AirtistApprovedTheme.Paper);
            foreach(var b in new[]{resultCollection,resultNext,resultReplay})
            {
                var label=GetButtonLabel(b);theme.Typography(label,page,b==resultNext?25:24);label.fontStyle=FontStyles.Bold;
                AirtistApprovedTheme.Rect(label.rectTransform,.04f,.10f,.92f,.80f);
            }
            GetButtonLabel(resultCollection).text="В мою галерею";
            theme.Picture(resultReplay.transform,"ReplayIcon",theme.SettingsIcon(2),.055f,.12f,.105f,.76f);
            AirtistApprovedTheme.Rect(GetButtonLabel(resultReplay).rectTransform,.18f,.10f,.78f,.80f);
            void Place(TMP_Text text,float x,float y,float w,float h,float size,bool bold=false)
            {AirtistApprovedTheme.Rect(text.rectTransform,x,y,w,h);theme.Typography(text,page,size);text.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;}
            void RewardTile(string name,Sprite icon,TMP_Text value,float x,Color tint)
            {
                var tile=theme.Picture(page,name,theme.surface,x,.640f,.222f,.068f,false);
                theme.Surface(tile,Color.Lerp(AirtistApprovedTheme.Paper,tint,.32f));tile.transform.SetSiblingIndex(value.transform.GetSiblingIndex());
                theme.Picture(tile.transform,"Icon",icon,.065f,.08f,.18f,.84f);
                Place(value,x+.059f,.652f,.154f,.043f,25,true);
            }
        }
        private void ApplyApprovedGift(AirtistApprovedTheme theme)
        {
            if(!pages.TryGetValue(Page.DailyBonus,out var pageObject))return;
            var page=(RectTransform)pageObject.transform;
            var card=page.Find("DailyBonusCard")?.GetComponent<UnityEngine.UI.Image>();theme.Surface(card,AirtistApprovedTheme.Paper);theme.Wood(card);
            if(card!=null)AirtistApprovedTheme.Rect(card.rectTransform,.24f,.135f,.52f,.80f);
            AirtistApprovedTheme.Hide(page,"DailyReward");
            theme.Picture(page,"GiftBrush",theme.dailyGift!=null?theme.dailyGift:theme.Icon(7),.355f,.275f,.29f,.32f);
            Move("DailyHeading",.28f,.17f,.44f,.085f);Move("DailySubtitle",.29f,.68f,.42f,.045f);
            AirtistApprovedTheme.Hide(page,"DailyAmount");
            Move("DailyRewardCaption",.32f,.595f,.36f,.07f);Move("DailySchedule",.29f,.855f,.42f,.04f);
            var caption=page.Find("DailyRewardCaption")?.GetComponent<TMP_Text>();theme.Typography(caption,page,32);if(caption!=null){caption.text="+1 подсказка";caption.fontStyle=FontStyles.Bold;}
            var schedule=page.Find("DailySchedule")?.GetComponent<TMP_Text>();if(schedule!=null){schedule.text="Новый подарок каждый день в 00:00 UTC";theme.Typography(schedule,page,18);}
            AirtistApprovedTheme.Rect((RectTransform)dailyClaimButton.transform,.33f,.745f,.34f,.10f);theme.Button(dailyClaimButton,AirtistApprovedTheme.Sage);
            var heading=page.Find("DailyHeading")?.GetComponent<TMP_Text>();theme.Typography(heading,page,38);if(heading!=null)heading.fontStyle=FontStyles.Bold;
            theme.Typography(dailyClaimButtonLabel,page,32);
            void Move(string n,float x,float y,float w,float h){var t=page.Find(n) as RectTransform;if(t!=null)AirtistApprovedTheme.Rect(t,x,y,w,h);}
        }
    }
}
