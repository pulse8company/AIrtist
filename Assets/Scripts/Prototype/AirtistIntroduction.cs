using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        private static readonly string[] HeroNames={"Амели","Лео","Мика"};
        private static readonly string[] HeroStudies={"Живопись","Реставрация","История искусства"};
        private static readonly string[] HeroOpening={
            "Я учусь живописи в Париже. Сегодня иду в Лувр — посмотреть, как великие мастера оживляли свои картины.",
            "Я учусь реставрации в Париже. Сегодня в Лувре хочу рассмотреть детали, которые обычно остаются незамеченными.",
            "Я изучаю историю искусства в Париже. Сегодня проверю в Лувре экспериментальные AI-очки: говорят, с ними видишь больше."
        };
        private static readonly string[] StoryTitles={"Париж. Школа изящных искусств","Знакомая картина. Незнакомая деталь","Цифровой след","Расследование начинается"};
        private static readonly string[] StoryCopy={"",
            "Через AI-очки знакомая картина выглядит иначе. Подожди… этой детали здесь быть не должно!",
            "Дорисовки видны только через линзы. Их цифровой след ведёт к музейным камерам. Похоже, кто-то научил AI менять то, что мы видим.",
            "Такие же следы появляются в музеях по всему миру. Начну с Лувра: найду чужие детали и верну картинам их настоящий облик."
        };
        private int selectedHero,introVersion,introFrame=-1;
        private bool introReplay;
        private RectTransform introRoot,heroChoices,storyPage;
        private TMP_Text storyTitle,storyCopy,storyCounter,storyNextLabel,heroHeading,heroSubtitle,heroConfirmLabel,heroStoryLabel;
        private UnityEngine.UI.Button heroClose;
        private UnityEngine.UI.RawImage storyPicture;
        private UnityEngine.UI.Button storyBack;
        private readonly UnityEngine.UI.Button[] heroButtons=new UnityEngine.UI.Button[3];
        private AirtistIntroArt introArt;
        private bool IntroductionOpen => introRoot!=null && introRoot.gameObject.activeSelf;

        private void LoadIntroduction(AirtistProgress state)
        {
            selectedHero=Mathf.Clamp(state.hero,0,2);introVersion=state.introVersion;
            introFrame=Mathf.Clamp(state.introFrame,-1,3);
            // Existing profiles retain their route and never get forced back to Mona Lisa.
            if(introVersion==0 && state.chapters.Length>0)introVersion=1;
            else if(introVersion==0)introVersion=-1; // Distinguish resumable new players from pre-intro saves.
        }
        private void SaveIntroduction(AirtistProgress state)
        {state.hero=selectedHero;state.introVersion=introVersion;state.introFrame=introFrame;}

        private void BuildIntroduction()
        {
            var theme=AirtistApprovedTheme.Current;
            if(theme==null)return;
            introArt=Resources.Load<AirtistIntroArt>("IntroductionArt");
            introRoot=CreatePanel((RectTransform)transform,"Introduction",AirtistApprovedTheme.Paper,Anchor.Stretch,Vector2.zero,Vector2.zero);
            Stretch(introRoot);introRoot.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            theme.Picture(introRoot,"Studio",theme.homeBackdrop,0,0,1,1,false);
            var wash=theme.Picture(introRoot,"ReadingWash",theme.surface,.02f,.025f,.96f,.95f,false);
            theme.Surface(wash,new Color(.99f,.96f,.88f,.92f));
            theme.Wood(wash);
            heroChoices=CreatePanel(introRoot,"HeroChoices",Color.clear,Anchor.Stretch,Vector2.zero,Vector2.zero);Stretch(heroChoices);
            heroHeading=IntroLabel(heroChoices,"Heading","Кто начнёт расследование?",.10f,.06f,.80f,.08f,42);
            heroHeading.fontStyle=FontStyles.Bold;
            heroSubtitle=IntroLabel(heroChoices,"Subtitle","Выбери портрет — и начни свою историю в Лувре",.12f,.15f,.76f,.05f,24);
            for(int i=0;i<3;i++)
            {
                int hero=i;
                var b=IntroButton(heroChoices,"Hero"+i,"",.108f+i*.267f,.225f,.25f,.54f,()=>ChooseHero(hero));
                heroButtons[i]=b;
                b.transform.Find("EnamelFinish").gameObject.SetActive(false);
                theme.Wood(b.image);
                var card=(RectTransform)b.transform;
                var background=theme.Picture(card,"PortraitMat",theme.surface,.055f,.045f,.89f,.625f,false);
                theme.Surface(background,Color.Lerp(AirtistApprovedTheme.Paper,i==0?AirtistApprovedTheme.Honey:i==1?AirtistApprovedTheme.Sage:AirtistApprovedTheme.Lavender,.22f));
                var sprite=introArt!=null?introArt.Hero(i):null;
                if(sprite!=null)theme.Picture(card,"Portrait",sprite,.065f,.055f,.87f,.60f);
                else IntroImage(card,"Portrait",introArt!=null?introArt.portraits:null,
                    introArt!=null?introArt.Portrait(i):new Rect(0,0,1,1),.065f,.055f,.87f,.60f);
                var nameLabel=IntroLabel(card,"Name",HeroNames[i],.06f,.686f,.88f,.088f,32);nameLabel.fontStyle=FontStyles.Bold;
                IntroLabel(card,"Study",HeroStudies[i],.06f,.791f,.88f,.064f,22);
                var status=IntroLabel(card,"SelectionStatus","",.13f,.881f,.74f,.064f,19);status.fontStyle=FontStyles.Bold;
                var badge=theme.Picture(card,"SelectionBadge",theme.surface,.78f,.055f,.15f,.13f,false);
                theme.Surface(badge,AirtistApprovedTheme.Teal);
                theme.Picture(badge.transform,"Check",theme.SettingsIcon(8),.14f,.14f,.72f,.72f);
                badge.transform.SetAsLastSibling();
            }
            var confirm=IntroButton(heroChoices,"StartStory","Начать историю",.51f,.803f,.32f,.09f,ConfirmHeroChoice);
            heroConfirmLabel=GetButtonLabel(confirm);
            var story=IntroButton(heroChoices,"HeroStory","Сразу к обучению",.17f,.803f,.32f,.09f,()=>{
                if(introReplay)StartHeroStory();else FinishIntroduction();
            });heroStoryLabel=GetButtonLabel(story);
            IntroLabel(heroChoices,"FairChoice","Героя можно сменить позже. Прогресс и награды сохраняются.",.12f,.907f,.76f,.035f,19);
            heroClose=IntroButton(heroChoices,"CloseHeroChoices","×",.907f,.058f,.052f,.085f,CloseHeroChoices);
            storyPage=CreatePanel(introRoot,"Story",Color.clear,Anchor.Stretch,Vector2.zero,Vector2.zero);Stretch(storyPage);
            storyTitle=IntroLabel(storyPage,"Title","",.07f,.055f,.86f,.09f,39);
            storyPicture=IntroImage(storyPage,"ComicPanel",introArt!=null?introArt.comic:null,new Rect(0,0,1,1),.12f,.175f,.37f,.63f);
            storyCopy=IntroLabel(storyPage,"Narration","",.54f,.24f,.37f,.40f,32);
            storyCopy.alignment=TextAlignmentOptions.MidlineLeft;
            storyCounter=IntroLabel(storyPage,"Counter","",.57f,.67f,.31f,.055f,22);
            storyBack=IntroButton(storyPage,"Back","Назад",.10f,.865f,.18f,.075f,()=>{introFrame--;SaveProgress();RefreshStory();});
            IntroButton(storyPage,"Skip","Пропустить",.35f,.865f,.22f,.075f,FinishIntroduction);
            var next=IntroButton(storyPage,"Next","Далее",.64f,.855f,.26f,.09f,()=>{
                if(introFrame>=3)FinishIntroduction();else{introFrame++;SaveProgress();RefreshStory();}
            });storyNextLabel=GetButtonLabel(next);
            introRoot.gameObject.SetActive(false);
            // Replay/change hero without touching the player's paintings or balances.
            if(pages.TryGetValue(Page.Home,out var home))
            {
                IntroButton((RectTransform)home.transform,"ReplayIntroduction","Выбрать героя",.66f,.89f,.25f,.075f,()=>OpenIntroduction(true));
            }
            ApplyHeroAppearance();
        }
        private void ShowIntroductionIfNeeded()
        {if(introVersion<=0)OpenIntroduction(false);}
        private void OpenIntroduction(bool replay)
        {
            if(introRoot==null)return;
            introReplay=replay;
            if(replay)introFrame=-1;
            if(universalHeader!=null)universalHeader.gameObject.SetActive(false);
            introRoot.gameObject.SetActive(true);introRoot.SetAsLastSibling();RefreshStory();
        }
        private void ChooseHero(int hero)
        {selectedHero=Mathf.Clamp(hero,0,HeroNames.Length-1);SaveProgress();RefreshHeroChoices();ApplyHeroAppearance();}
        private void StartHeroStory()
        {introFrame=0;SaveProgress();RefreshStory();}
        private void ConfirmHeroChoice()
        {if(introReplay)CloseHeroChoices();else StartHeroStory();}
        private void CloseHeroChoices()
        {
            if(!introReplay)return;
            introFrame=-1;introRoot.gameObject.SetActive(false);ApplyHeroAppearance();SaveProgress();
            if(universalHeader!=null)universalHeader.gameObject.SetActive(true);
        }
        private void RefreshHeroChoices()
        {
            var theme=AirtistApprovedTheme.Current;
            for(int i=0;i<heroButtons.Length;i++)
            {
                var b=heroButtons[i];if(b==null)continue;
                bool chosen=i==selectedHero;
                theme.Surface(b.image,chosen?Color.Lerp(AirtistApprovedTheme.Paper,AirtistApprovedTheme.Sage,.35f):AirtistApprovedTheme.Paper);
                b.transform.Find("SelectionBadge").gameObject.SetActive(chosen);
                var status=b.transform.Find("SelectionStatus").GetComponent<TMP_Text>();
                status.text=chosen?"Твой герой":"Выбрать";status.color=chosen?AirtistApprovedTheme.Teal:AirtistApprovedTheme.Ink;
                var edge=b.transform.Find("OakFrame")?.GetComponent<UnityEngine.UI.Image>();
                if(edge!=null)edge.color=chosen?new Color(.70f,.88f,.80f):Color.white;
            }
            heroHeading.text=introReplay?"Выбери своего героя":"Кто начнёт расследование?";
            heroSubtitle.text=introReplay?"Коснись портрета — выбор сохранится автоматически":"Выбери портрет — и начни свою историю в Лувре";
            heroConfirmLabel.text=introReplay?"Продолжить · "+HeroNames[selectedHero]:"Начать историю";
            heroStoryLabel.text=introReplay?"История героя":"Сразу к обучению";
            heroClose.gameObject.SetActive(introReplay);
        }
        private void RefreshStory()
        {
            bool choosing=introFrame<0;heroChoices.gameObject.SetActive(choosing);storyPage.gameObject.SetActive(!choosing);
            RefreshHeroChoices();if(choosing)return;
            storyTitle.text=StoryTitles[introFrame];storyCopy.text=introFrame==0?HeroOpening[selectedHero]:StoryCopy[introFrame];
            storyCounter.text=HeroNames[selectedHero]+" · "+(introFrame+1)+" / 4";
            if(introArt!=null)
            {
                storyPicture.uvRect=introArt.Frame(selectedHero,introFrame);
                var uv=storyPicture.uvRect;var texture=storyPicture.texture;
                var fit=storyPicture.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                if(fit!=null && texture!=null)fit.aspectRatio=texture.width*uv.width/(texture.height*uv.height);
            }
            storyNextLabel.text=introFrame==3?(introReplay?"Вернуться в игру":"Начать поиск"):"Далее";
        }
        private void FinishIntroduction()
        {
            if(introReplay){CloseHeroChoices();return;}
            introVersion=1;introFrame=-1;introRoot.gameObject.SetActive(false);ApplyHeroAppearance();
            if(universalHeader!=null)universalHeader.gameObject.SetActive(true);
            BeginMonaTutorial();
        }
        private void ApplyHeroAppearance()
        {
            if(introArt==null)return;
            var heroSprite=introArt.Hero(selectedHero);
            foreach(var image in GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if(image.name!="ApprovedAmelie")continue;
                var parent=(RectTransform)image.transform.parent;
                var existing=parent.Find("SelectedHeroSlot/SelectedHero")?.GetComponent<UnityEngine.UI.RawImage>();
                if(heroSprite!=null)
                {
                    image.sprite=heroSprite;image.enabled=true;
                    if(existing!=null)existing.transform.parent.gameObject.SetActive(false);
                    continue;
                }
                if(introArt.portraits==null)continue;
                if(existing==null)existing=IntroImage(parent,"SelectedHero",introArt.portraits,introArt.Portrait(selectedHero),0,0,1,1);
                var from=image.rectTransform;var to=(RectTransform)existing.transform.parent;
                to.anchorMin=from.anchorMin;to.anchorMax=from.anchorMax;to.pivot=from.pivot;
                to.offsetMin=from.offsetMin;to.offsetMax=from.offsetMax;
                existing.texture=introArt.portraits;existing.uvRect=introArt.Portrait(selectedHero);
                to.SetSiblingIndex(image.transform.GetSiblingIndex()+1);
                var fit=existing.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                if(fit!=null)fit.aspectRatio=introArt.portraits.width*existing.uvRect.width/(introArt.portraits.height*existing.uvRect.height);
                to.gameObject.SetActive(true);image.enabled=false;
            }
            foreach(var gallery in new[]{myGallery,museumGallery})
            {
                if(gallery==null)continue;
                var name=gallery.transform.Find("ProfilePanel/PlayerName")?.GetComponent<TMP_Text>();
                if(name!=null)name.text=HeroNames[selectedHero];
            }
            if(pages.TryGetValue(Page.Home,out var home))
            {
                var change=home.transform.Find("ReplayIntroduction")?.GetComponent<UnityEngine.UI.Button>();
                var name=GetButtonLabel(change);
                if(name!=null)name.text=HeroNames[selectedHero]+" · сменить героя";
            }
        }
        private TMP_Text IntroLabel(RectTransform root,string name,string value,float x,float y,float w,float h,float size)
        {
            var label=CreateLabel(root,value,size,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,TextAlignmentOptions.Center);
            label.name=name;AirtistApprovedTheme.Rect(label.rectTransform,x,y,w,h);
            AirtistApprovedTheme.Current.Typography(label,(RectTransform)transform,size);
            return label;
        }
        private UnityEngine.UI.Button IntroButton(RectTransform root,string name,string value,float x,float y,float w,float h,Action click)
        {
            var button=CreateButton(root,value,AirtistApprovedTheme.Paper,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,click,24);
            button.name=name;AirtistApprovedTheme.Rect((RectTransform)button.transform,x,y,w,h);
            AirtistApprovedTheme.Current.Button(button,name=="Next"||name=="StartStory"?AirtistApprovedTheme.Coral:AirtistApprovedTheme.Paper);
            var text=GetButtonLabel(button);AirtistApprovedTheme.Rect(text.rectTransform,.03f,.08f,.94f,.84f);
            AirtistApprovedTheme.Current.Typography(text,(RectTransform)transform,25);return button;
        }
        private UnityEngine.UI.RawImage IntroImage(RectTransform root,string name,Texture texture,Rect uv,float x,float y,float w,float h)
        {
            var slot=new GameObject(name+"Slot",typeof(RectTransform)).GetComponent<RectTransform>();slot.SetParent(root,false);
            AirtistApprovedTheme.Rect(slot,x,y,w,h);
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.RawImage));go.transform.SetParent(slot,false);
            var image=go.GetComponent<UnityEngine.UI.RawImage>();image.texture=texture;image.uvRect=uv;image.raycastTarget=false;
            AirtistApprovedTheme.Rect(image.rectTransform,0,0,1,1);
            if(texture!=null)
            {
                var fit=go.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
                fit.aspectRatio=(texture.width*uv.width)/(texture.height*uv.height);
            }
            return image;
        }
    }
}
