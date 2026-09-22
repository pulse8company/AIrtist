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
            var bar=theme.Picture(universalHeader,"HeaderPaper",theme.surface,0,0,1,.08f,false);
            theme.Surface(bar,AirtistApprovedTheme.Paper);bar.raycastTarget=true;
            Label("Brand","AIrtist",.025f,.008f,.13f,.065f,48);
            theme.Picture(universalHeader,"EnergyIcon",theme.Icon(8),.162f,.014f,.023f,.052f);
            headerEnergy=Label("Energy","",.187f,.018f,.075f,.044f,27);
            theme.Picture(universalHeader,"CoinIcon",theme.Icon(9),.286f,.014f,.030f,.052f);
            headerCoins=Label("Coins","",.320f,.018f,.074f,.044f,27);
            Nav("Gift","",7,.538f,.050f,OpenDailyBonus);
            Nav("Map","Карта",10,.600f,.100f,()=>Show(Page.WorldMap));
            Nav("Collection","Моя галерея",11,.705f,.150f,()=>Show(Page.Collection));
            Nav("Store","Магазин",12,.858f,.095f,()=>Show(Page.Store));
            // Fit Home / X without overlapping the store on small landscape screens.
            var store=universalHeader.Find("Store") as RectTransform;AirtistApprovedTheme.Rect(store,.815f,.008f,.10f,.064f);
            var collection=universalHeader.Find("Collection") as RectTransform;AirtistApprovedTheme.Rect(collection,.680f,.008f,.13f,.064f);
            var map=universalHeader.Find("Map") as RectTransform;AirtistApprovedTheme.Rect(map,.584f,.008f,.090f,.064f);
            var gift=universalHeader.Find("Gift") as RectTransform;AirtistApprovedTheme.Rect(gift,.536f,.008f,.043f,.064f);
            universalHome=Nav("Home","",13,.920f,.038f,()=>Show(Page.Home)).gameObject;
            universalClose=Nav("Close","×",-1,.960f,.037f,()=>Show(Page.Home)).gameObject;
            RefreshHeaderWallet();
            TextMeshProUGUI Label(string name,string text,float x,float y,float w,float h,float size)
            {var t=CreateLabel(universalHeader,text,(int)size,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,TextAlignmentOptions.Left);t.name=name;AirtistApprovedTheme.Rect(t.rectTransform,x,y,w,h);theme.Typography(t,universalHeader,size);return t;}
            UnityEngine.UI.Button Nav(string name,string text,int icon,float x,float w,Action action)
            {
                var b=CreateButton(universalHeader,text,AirtistApprovedTheme.Paper,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,action,22);b.name=name;
                AirtistApprovedTheme.Rect((RectTransform)b.transform,x,.008f,w,.064f);theme.Button(b,AirtistApprovedTheme.Paper);
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
            foreach(var pair in pages)
            {
                theme.Common(pair.Value.transform);
                if(pair.Key==Page.WorldMap || pair.Key==Page.Home)continue;
                var bg=theme.Picture(pair.Value.transform,"ApprovedMuseumBackdrop",theme.museumBackdrop,0,0,1,1,false);bg.transform.SetAsFirstSibling();
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
                for(int i=3;i<5;i++){theme.Button(map.Controls[i],AirtistApprovedTheme.Paper);AirtistApprovedTheme.Rect((RectTransform)map.Controls[i].transform,.79f+(i-3)*.065f,.88f,.060f,.10f);}
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
            if(paper!=null)AirtistApprovedTheme.Rect(paper.rectTransform,.46f,.10f,.525f,.865f);
            AirtistApprovedTheme.Rect(resultPainting.rectTransform,.075f,.105f,.345f,.67f);
            AirtistApprovedTheme.Rect(resultPaintingTitle.rectTransform,.10f,.785f,.30f,.09f);
            AirtistApprovedTheme.Rect(resultHeading.rectTransform,.49f,.12f,.47f,.10f);theme.Typography(resultHeading,page,36);
            for(int i=0;i<3;i++)
            {AirtistApprovedTheme.Rect(resultStars[i].rectTransform,.52f+i*.13f,.245f,.11f,.16f);var caption=page.Find("Criterion"+i) as RectTransform;if(caption!=null)AirtistApprovedTheme.Rect(caption,.51f+i*.13f,.41f,.13f,.055f);}
            AirtistApprovedTheme.Rect(resultMetrics.rectTransform,.49f,.49f,.46f,.105f);
            AirtistApprovedTheme.Rect(resultRewards.rectTransform,.50f,.62f,.45f,.08f);
            AirtistApprovedTheme.Rect(resultFact.rectTransform,.50f,.725f,.45f,.105f);
            foreach(var b in page.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                var label=GetButtonLabel(b);if(label.text=="В коллекцию"){AirtistApprovedTheme.Rect((RectTransform)b.transform,.485f,.85f,.20f,.09f);theme.Button(b,AirtistApprovedTheme.Sage);}
                else if(b==resultNext){AirtistApprovedTheme.Rect((RectTransform)b.transform,.695f,.85f,.27f,.09f);theme.Button(b,AirtistApprovedTheme.Coral);}
                else {AirtistApprovedTheme.Rect((RectTransform)b.transform,.12f,.885f,.29f,.07f);theme.Button(b,AirtistApprovedTheme.Paper);}
            }
        }
        private void ApplyApprovedGift(AirtistApprovedTheme theme)
        {
            if(!pages.TryGetValue(Page.DailyBonus,out var pageObject))return;
            var page=(RectTransform)pageObject.transform;
            var card=page.Find("DailyBonusCard")?.GetComponent<UnityEngine.UI.Image>();theme.Surface(card,AirtistApprovedTheme.Paper);
            if(card!=null)AirtistApprovedTheme.Rect(card.rectTransform,.20f,.12f,.60f,.80f);
            AirtistApprovedTheme.Hide(page,"DailyReward");
            theme.Picture(page,"GiftBrush",theme.Icon(3),.39f,.29f,.22f,.28f);
            Move("DailyHeading",.25f,.16f,.50f,.085f);Move("DailySubtitle",.27f,.65f,.46f,.05f);
            Move("DailyAmount",.42f,.555f,.16f,.055f);Move("DailyRewardCaption",.40f,.605f,.20f,.045f);Move("DailySchedule",.29f,.84f,.42f,.065f);
            AirtistApprovedTheme.Rect((RectTransform)dailyClaimButton.transform,.33f,.73f,.34f,.10f);theme.Button(dailyClaimButton,AirtistApprovedTheme.Coral);
            void Move(string n,float x,float y,float w,float h){var t=page.Find(n) as RectTransform;if(t!=null)AirtistApprovedTheme.Rect(t,x,y,w,h);}
        }
    }
}
