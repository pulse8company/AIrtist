using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        private const string SoundPreference="AIrtist.Settings.Sound", LocalePreference="AIrtist.Settings.Locale";
        private RectTransform settingsRoot, settingsPaper, settingsDialog;
        private Button settingsHeaderButton, settingsSoundButton;
        private TMP_Text settingsSoundState, settingsLanguage, settingsDialogTitle, settingsDialogBody;
        private RectTransform settingsDialogActions;
        private Image settingsSoundKnob;
        private Sprite settingsKnobSprite;
        private Sprite settingsPreviousBackdrop;
        private bool settingsPanZoomEnabled;
        private readonly Button[] settingsSocialButtons=new Button[2];
        private readonly Dictionary<Button,bool> settingsHeaderStates=new Dictionary<Button,bool>();
        private readonly Dictionary<CanvasGroup,(float alpha,bool interactable,bool raycasts)> settingsHiddenUI=new Dictionary<CanvasGroup,(float,bool,bool)>();
        private bool settingsHomeVisible, settingsCloseVisible, settingsPrivacyBusy;
        private int socialRewardsClaimed, socialPagesOpened;
        private bool SettingsOpen=>settingsRoot!=null && settingsRoot.gameObject.activeSelf;
        private bool GameSoundEnabled=>PlayerPrefs.GetInt(SoundPreference,1)!=0;
        private AirtistSettingsConfig SettingsConfig=>AirtistSettingsConfig.Current;

        private void InitializeSettingsPreferences()
        {
            AudioListener.volume=GameSoundEnabled?1:0;
            StartCoroutine(RestoreSettingsLocale());
        }
        private IEnumerator RestoreSettingsLocale()
        {
            yield return LocalizationSettings.InitializationOperation;
            if(!LocalizationSettings.InitializationOperation.IsValid()
                || LocalizationSettings.InitializationOperation.Status!=UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)yield break;
            string saved=PlayerPrefs.GetString(LocalePreference,"");
            if(string.IsNullOrEmpty(saved))yield break;
            var locale=LocalizationSettings.AvailableLocales.GetLocale(saved);
            if(locale!=null && IsPublishedLocale(locale))LocalizationSettings.SelectedLocale=locale;
        }
        private bool IsPublishedLocale(Locale locale)
        {
            var codes=SettingsConfig!=null?SettingsConfig.publishedLocaleCodes:null;
            return locale!=null && (codes!=null?Array.IndexOf(codes,locale.Identifier.Code)>=0:locale.Identifier.Code=="ru");
        }

        private void BuildSettings()
        {
            var theme=AirtistApprovedTheme.Current;if(theme==null)return;
            settingsRoot=CreatePanel((RectTransform)transform,"SettingsWindow",new Color(0,0,0,.36f),Anchor.Stretch,Vector2.zero,Vector2.zero);
            Stretch(settingsRoot);settingsRoot.GetComponent<Image>().raycastTarget=true;
            settingsPaper=CreatePanel(settingsRoot,"SettingsPaper",AirtistApprovedTheme.Paper,Anchor.Center,Vector2.zero,Vector2.one);
            AirtistApprovedTheme.Rect(settingsPaper,.125f,.13f,.815f,.835f);
            theme.Surface(settingsPaper.GetComponent<Image>(),AirtistApprovedTheme.Paper);theme.Wood(settingsPaper.GetComponent<Image>());
            theme.Picture(settingsPaper,"TitleGear",theme.settingsGear,.325f,.035f,.075f,.13f);
            SettingsLabel(settingsPaper,"Title","Настройки",.41f,.035f,.36f,.13f,48).fontStyle=FontStyles.Bold;
            SettingsLabel(settingsPaper,"GameHeading","ИГРА И ПОМОЩЬ",.045f,.17f,.42f,.07f,25).fontStyle=FontStyles.Bold;
            SettingsLabel(settingsPaper,"CommunityHeading","СООБЩЕСТВО И ДАННЫЕ",.525f,.17f,.43f,.07f,25).fontStyle=FontStyles.Bold;
            var divider=theme.Picture(settingsPaper,"ColumnDivider",whiteSprite,.499f,.22f,.001f,.60f,false);divider.color=new Color(.67f,.49f,.25f,.45f);
            settingsSoundButton=SettingsRow(settingsPaper,"Sound","Звук",0,.045f,.25f,.43f,.13f,ToggleGameSound);
            var soundPill=theme.Picture(settingsSoundButton.transform,"SoundSwitch",theme.surface,.71f,.19f,.25f,.62f,false);theme.Surface(soundPill,AirtistApprovedTheme.Sage);
            settingsKnobSprite=CreateRoundedSprite(31f);
            settingsSoundKnob=theme.Picture(settingsSoundButton.transform,"SoundKnob",settingsKnobSprite,.86f,.25f,.075f,.5f);settingsSoundKnob.color=AirtistApprovedTheme.Paper;
            settingsSoundState=SettingsLabel((RectTransform)settingsSoundButton.transform,"SoundState","ВКЛ",.72f,.15f,.14f,.70f,21,TextAlignmentOptions.Center);
            var languageButton=SettingsRow(settingsPaper,"Language","Язык",1,.045f,.40f,.43f,.13f,OpenLanguageSettings);
            settingsLanguage=SettingsLabel((RectTransform)languageButton.transform,"CurrentLanguage","Русский  ›",.57f,.15f,.37f,.7f,24,TextAlignmentOptions.Right);
            SettingsRow(settingsPaper,"RestorePurchases","Восстановить покупки",2,.045f,.55f,.43f,.13f,RestoreFromSettings,true);
            SettingsRow(settingsPaper,"Support","Поддержка",3,.045f,.70f,.43f,.13f,OpenSettingsSupport,true);
            for(int i=0;i<2;i++)
            {
                int network=i;
                var row=SettingsRow(settingsPaper,i==0?"Instagram":"Facebook",i==0?"Instagram":"Facebook",4+i,.525f,.25f+i*.15f,.43f,.13f,()=>OpenSocialPage(network));
                AirtistApprovedTheme.Rect(GetButtonLabel(row).rectTransform,.185f,.1f,.31f,.8f);
                theme.Typography(GetButtonLabel(row),(RectTransform)transform,25);
                theme.Picture(row.transform,"Coin",theme.Icon(9),.485f,.24f,.075f,.52f);
                SettingsLabel((RectTransform)row.transform,"Reward","+"+(SettingsConfig!=null?SettingsConfig.socialRewardCoins:50),.56f,.15f,.105f,.7f,23);
                settingsSocialButtons[i]=SettingsAction((RectTransform)row.transform,"Follow","Подписаться",.68f,.16f,.29f,.68f,AirtistApprovedTheme.Sage,()=>OpenSocialPage(network),21);
            }
            var privacy=SettingsRow(settingsPaper,"Privacy","Политика\nконфиденциальности",6,.525f,.55f,.43f,.13f,OpenPrivacyPolicy,true);
            theme.Typography(GetButtonLabel(privacy),(RectTransform)transform,25);
            SettingsRow(settingsPaper,"AdsPrivacy","Реклама и данные",7,.525f,.70f,.43f,.13f,OpenAdSettings,true);
            var line=theme.Picture(settingsPaper,"FooterDivider",whiteSprite,.045f,.855f,.91f,.002f,false);line.color=divider.color;
            var version=SettingsAction(settingsPaper,"Version",AirtistBuildInfo.Description,.045f,.89f,.68f,.07f,Color.clear,CopyBuildInformation,20);
            version.image.color=Color.clear;GetButtonLabel(version).alignment=TextAlignmentOptions.Left;
            SettingsAction(settingsPaper,"Done","Готово",.745f,.875f,.21f,.09f,AirtistApprovedTheme.Coral,CloseSettings,29);

            settingsDialog=CreatePanel(settingsRoot,"SettingsDetail",new Color(0,0,0,.45f),Anchor.Stretch,Vector2.zero,Vector2.zero);Stretch(settingsDialog);
            var detail=CreatePanel(settingsDialog,"Paper",AirtistApprovedTheme.Paper,Anchor.Center,Vector2.zero,Vector2.one);
            AirtistApprovedTheme.Rect(detail,.24f,.21f,.57f,.68f);theme.Surface(detail.GetComponent<Image>(),AirtistApprovedTheme.Paper);theme.Wood(detail.GetComponent<Image>());
            settingsDialogTitle=SettingsLabel(detail,"Title","",.065f,.055f,.79f,.15f,36);
            settingsDialogBody=SettingsLabel(detail,"Body","",.065f,.24f,.87f,.27f,24);
            SettingsAction(detail,"Close","×",.87f,.04f,.085f,.14f,AirtistApprovedTheme.Coral,CloseSettingsDetail,32);
            settingsDialogActions=new GameObject("Actions",typeof(RectTransform)).GetComponent<RectTransform>();settingsDialogActions.SetParent(detail,false);
            AirtistApprovedTheme.Rect(settingsDialogActions,.065f,.56f,.87f,.37f);
            settingsDialog.gameObject.SetActive(false);settingsRoot.gameObject.SetActive(false);
        }
        private TMP_Text SettingsLabel(RectTransform parent,string name,string text,float x,float y,float w,float h,float size,TextAlignmentOptions alignment=TextAlignmentOptions.Left)
        {
            var label=CreateLabel(parent,text,size,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,alignment);label.name=name;
            AirtistApprovedTheme.Rect(label.rectTransform,x,y,w,h);AirtistApprovedTheme.Current?.Typography(label,(RectTransform)transform,size);return label;
        }
        private Button SettingsAction(RectTransform parent,string name,string text,float x,float y,float w,float h,Color color,Action action,float size=26)
        {
            var button=CreateButton(parent,text,color,AirtistApprovedTheme.Ink,Anchor.Center,Vector2.zero,Vector2.one,action,(int)size);button.name=name;
            AirtistApprovedTheme.Rect((RectTransform)button.transform,x,y,w,h);AirtistApprovedTheme.Current?.Button(button,color);
            var label=GetButtonLabel(button);AirtistApprovedTheme.Rect(label.rectTransform,.04f,.08f,.92f,.84f);AirtistApprovedTheme.Current?.Typography(label,(RectTransform)transform,size);
            return button;
        }
        private Button SettingsRow(RectTransform parent,string name,string text,int icon,float x,float y,float w,float h,Action action,bool chevron=false)
        {
            var b=SettingsAction(parent,name,text,x,y,w,h,AirtistApprovedTheme.Paper,action,27);
            var theme=AirtistApprovedTheme.Current;theme.Picture(b.transform,"Icon",theme.SettingsIcon(icon),.025f,.08f,.15f,.84f);
            AirtistApprovedTheme.Rect(GetButtonLabel(b).rectTransform,.19f,.08f,chevron?.71f:.49f,.84f);GetButtonLabel(b).alignment=TextAlignmentOptions.Left;
            if(chevron)SettingsLabel((RectTransform)b.transform,"Chevron","›",.90f,.1f,.07f,.8f,34,TextAlignmentOptions.Center);
            return b;
        }
        private void ToggleSettings(){if(SettingsOpen)CloseSettings();else OpenSettings();}
        private void CloseTopWindow(){if(SettingsOpen){if(settingsDialog.gameObject.activeSelf)CloseSettingsDetail();else CloseSettings();}else Show(Page.Home);}
        private void OpenSettings()
        {
            if(adPending || IntroductionOpen || settingsRoot==null || SettingsOpen)return;
            TickAttemptClock();if(energyShop!=null)energyShop.gameObject.SetActive(false);
            settingsHomeVisible=universalHome!=null && universalHome.activeSelf;settingsCloseVisible=universalClose!=null && universalClose.activeSelf;
            settingsRoot.gameObject.SetActive(true);settingsRoot.SetAsLastSibling();settingsDialog.gameObject.SetActive(false);
            // Hide the underlying screen without deactivating it or losing its scroll/zoom state.
            settingsHiddenUI.Clear();
            foreach(Transform child in transform)
            {
                if(child==settingsRoot || child==universalHeader || !child.gameObject.activeSelf)continue;
                var group=child.GetComponent<CanvasGroup>();if(group==null)group=child.gameObject.AddComponent<CanvasGroup>();
                settingsHiddenUI[group]=(group.alpha,group.interactable,group.blocksRaycasts);
                group.alpha=0;group.interactable=false;group.blocksRaycasts=false;
            }
            if(fullBleedHome!=null)
            {
                var image=fullBleedHome.GetComponent<Image>();settingsPreviousBackdrop=image.sprite;
                image.sprite=AirtistApprovedTheme.Current.museumBackdrop;
                fullBleedHome.GetComponent<AspectRatioFitter>().aspectRatio=image.sprite.rect.width/image.sprite.rect.height;
            }
            if(galleryPanZoom!=null){settingsPanZoomEnabled=galleryPanZoom.enabled;galleryPanZoom.enabled=false;}
            settingsHeaderStates.Clear();
            if(universalHeader!=null)
            {
                foreach(var button in universalHeader.GetComponentsInChildren<Button>(true))
                {
                    settingsHeaderStates[button]=button.interactable;
                    button.interactable=button==settingsHeaderButton || button.gameObject==universalHome || button.gameObject==universalClose;
                }
                universalHome?.SetActive(true);universalClose?.SetActive(true);universalHeader.SetAsLastSibling();
            }
            AirtistApprovedTheme.Current?.Button(settingsHeaderButton,AirtistApprovedTheme.Sage);
            RefreshSettings();RefreshAttemptHud();
        }
        private void CloseSettings()
        {
            if(!SettingsOpen || settingsPrivacyBusy)return;
            settingsRoot.gameObject.SetActive(false);settingsDialog.gameObject.SetActive(false);clockStamp=Time.realtimeSinceStartupAsDouble;
            foreach(var pair in settingsHiddenUI)if(pair.Key!=null){pair.Key.alpha=pair.Value.alpha;pair.Key.interactable=pair.Value.interactable;pair.Key.blocksRaycasts=pair.Value.raycasts;}
            settingsHiddenUI.Clear();
            if(fullBleedHome!=null && settingsPreviousBackdrop!=null)
            {
                fullBleedHome.GetComponent<Image>().sprite=settingsPreviousBackdrop;
                fullBleedHome.GetComponent<AspectRatioFitter>().aspectRatio=settingsPreviousBackdrop.rect.width/settingsPreviousBackdrop.rect.height;
            }
            if(galleryPanZoom!=null)galleryPanZoom.enabled=settingsPanZoomEnabled;
            foreach(var pair in settingsHeaderStates)if(pair.Key!=null)pair.Key.interactable=pair.Value;
            settingsHeaderStates.Clear();universalHome?.SetActive(settingsHomeVisible);universalClose?.SetActive(settingsCloseVisible);
            AirtistApprovedTheme.Current?.Button(settingsHeaderButton,AirtistApprovedTheme.Paper);RefreshAttemptHud();
        }
        private void UpdateSettingsInput()
        {
            if(!SettingsOpen)return;
            if(UnityEngine.InputSystem.Keyboard.current!=null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)CloseTopWindow();
        }
        private void ToggleGameSound()
        {
            if(!Application.isPlaying || !initialized)return;
            bool enabled=!GameSoundEnabled;PlayerPrefs.SetInt(SoundPreference,enabled?1:0);PlayerPrefs.Save();AudioListener.volume=enabled?1:0;RefreshSettings();
        }
        private void RefreshSettings()
        {
            settingsSoundState.text=GameSoundEnabled?"ВКЛ":"ВЫКЛ";
            AirtistApprovedTheme.Rect(settingsSoundKnob.rectTransform,GameSoundEnabled?.86f:.735f,.25f,.075f,.5f);
            AirtistApprovedTheme.Rect(settingsSoundState.rectTransform,GameSoundEnabled?.72f:.81f,.15f,.14f,.70f);
            var pill=settingsSoundButton.transform.Find("SoundSwitch")?.GetComponent<Image>();if(pill!=null)pill.color=GameSoundEnabled?AirtistApprovedTheme.Sage:new Color(.75f,.74f,.68f);
            var locale=LocalizationSettings.SelectedLocale;settingsLanguage.text=(locale==null || locale.Identifier.Code=="ru"?"Русский":locale.Identifier.CultureInfo?.NativeName??locale.LocaleName)+"  ›";
            for(int i=0;i<2;i++)
            {
                bool claimed=(socialRewardsClaimed&(1<<i))!=0, available=AirtistSettingsConfig.IsSocialUrl(SocialUrl(i),i);
                GetButtonLabel(settingsSocialButtons[i]).text=claimed?"Получено":!available?"Скоро":(socialPagesOpened&(1<<i))!=0?"Забрать":"Подписаться";
                settingsSocialButtons[i].image.color=claimed || !available?AirtistApprovedTheme.Paper:AirtistApprovedTheme.Sage;
            }
        }
        private void OpenSettingsDetail(string title,string body)
        {
            settingsDialogTitle.text=title;settingsDialogBody.text=body;
            for(int i=settingsDialogActions.childCount-1;i>=0;i--){var child=settingsDialogActions.GetChild(i).gameObject;child.SetActive(false);if(Application.isPlaying)Destroy(child);else DestroyImmediate(child);}
            settingsDialog.gameObject.SetActive(true);settingsDialog.SetAsLastSibling();
        }
        private void CloseSettingsDetail(){if(!settingsPrivacyBusy)settingsDialog.gameObject.SetActive(false);}
        private Button DetailAction(string text,int index,int count,Action action,Color? color=null)
        {
            float height=Mathf.Min(.47f,(1f-.06f*(count-1))/count);
            return SettingsAction(settingsDialogActions,"Action"+index,text,count==1?.46f:0,count==1?.53f:index*(height+.06f),count==1?.54f:1,height,color??AirtistApprovedTheme.Paper,action,25);
        }
        private void SettingsNotice(string title,string body)
        {OpenSettingsDetail(title,body);DetailAction("Понятно",0,1,CloseSettingsDetail,AirtistApprovedTheme.Sage);}
        private void OpenLanguageSettings()
        {
            if(!LocalizationSettings.InitializationOperation.IsDone){SettingsNotice("Язык","Список языков загружается. Попробуйте ещё раз через несколько секунд.");return;}
            var locales=new List<Locale>();foreach(var locale in LocalizationSettings.AvailableLocales.Locales)if(IsPublishedLocale(locale))locales.Add(locale);
            OpenSettingsDetail("Язык игры",locales.Count<=1?"Сейчас доступен русский язык. Другие языки появятся после подготовки переводов.":"Выберите язык игры. Изменение сохранится на этом устройстве.");
            for(int i=0;i<locales.Count;i++)
            {
                var selected=locales[i];string caption=selected.Identifier.Code=="ru"?"Русский":selected.Identifier.CultureInfo?.NativeName??selected.LocaleName;
                if(selected==LocalizationSettings.SelectedLocale)caption+="  ✓";
                DetailAction(caption,i,locales.Count,()=>
                {
                    if(!Application.isPlaying || !initialized)return;
                    LocalizationSettings.SelectedLocale=selected;PlayerPrefs.SetString(LocalePreference,selected.Identifier.Code);PlayerPrefs.Save();RefreshSettings();CloseSettingsDetail();
                },AirtistApprovedTheme.Sage);
            }
        }
        private void OpenPrivacyPolicy()
        {
            string url=SettingsConfig!=null?SettingsConfig.privacyPolicyUrl:"https://www.pulse8gaming.com/privacy-policy";
            if(AirtistSettingsConfig.IsWebUrl(url) && Application.isPlaying)Application.OpenURL(url);
            else SettingsNotice("Политика конфиденциальности","Ссылка доступна в игре. Адрес: https://www.pulse8gaming.com/privacy-policy");
        }
        private void CopyBuildInformation()
        {GUIUtility.systemCopyBuffer=AirtistBuildInfo.Description;SettingsNotice("Информация о сборке",AirtistBuildInfo.Description+"\nСкопировано. Можно отправить в поддержку.");}
        private void OpenSettingsSupport()
        {
            string email=SettingsConfig!=null?SettingsConfig.supportEmail:"";
            if(string.IsNullOrWhiteSpace(email)){SettingsNotice("Поддержка","Контакт поддержки скоро появится.\n"+AirtistBuildInfo.Description);return;}
            try
            {
                var address=new System.Net.Mail.MailAddress(email);
                if(address.Address!=email)throw new FormatException();
                if(Application.isPlaying)Application.OpenURL("mailto:"+address.Address+"?subject="+Uri.EscapeDataString("AIrtist — поддержка")+"&body="+Uri.EscapeDataString("Опишите проблему:\n\n\n"+AirtistBuildInfo.Description));
            }
            catch(FormatException){SettingsNotice("Поддержка","Адрес поддержки пока недоступен. Попробуйте позднее.");}
        }
        private string SocialUrl(int network)=>SettingsConfig==null?"":network==0?SettingsConfig.instagramUrl:SettingsConfig.facebookUrl;
        private void OpenSocialPage(int network)
        {
            string title=network==0?"Instagram":"Facebook";
            if(!AirtistSettingsConfig.IsSocialUrl(SocialUrl(network),network)){SettingsNotice(title,"Наша страница скоро появится. Награду можно будет получить после её подключения.");return;}
            if((socialRewardsClaimed&(1<<network))!=0){SettingsNotice(title,"Разовая награда уже получена. Спасибо, что вы с нами!");return;}
            if((socialPagesOpened&(1<<network))!=0)
            {
                OpenSettingsDetail(title,"Вы подписались на нашу страницу? Подтвердите подписку, чтобы забрать разовую награду.");
                DetailAction("Я подписался · +"+SettingsConfig.socialRewardCoins+" монет",0,2,()=>ClaimSocialReward(network),AirtistApprovedTheme.Sage);
                DetailAction("Открыть страницу ещё раз",1,2,()=>VisitSocialPage(network));return;
            }
            OpenSettingsDetail(title,"Откроется наша страница. Подпишитесь, затем вернитесь в игру и нажмите «Забрать». Награда выдаётся один раз.");
            DetailAction("Открыть "+title,0,1,()=>VisitSocialPage(network),AirtistApprovedTheme.Sage);
        }
        private void VisitSocialPage(int network)
        {
            if(!Application.isPlaying || !initialized || !AirtistSettingsConfig.IsSocialUrl(SocialUrl(network),network))return;
            // A visit is not verification. Claim requires a separate explicit self-confirmation.
            socialPagesOpened|=1<<network;Application.OpenURL(SocialUrl(network));CloseSettingsDetail();RefreshSettings();
        }
        private void ClaimSocialReward(int network)
        {
            if(!Application.isPlaying || !initialized || SettingsConfig==null || network<0 || network>1
                || !AirtistSettingsConfig.IsSocialUrl(SocialUrl(network),network) || (socialPagesOpened&(1<<network))==0
                || (socialRewardsClaimed&(1<<network))!=0)return;
            int amount=Mathf.Max(0,SettingsConfig.socialRewardCoins);
            if(coins>int.MaxValue-amount){SettingsNotice("Награда","Запас монет достиг максимума.");return;}
            // Claim flag and coins share one progress snapshot, including normal autosaves.
            socialRewardsClaimed|=1<<network;coins+=amount;SaveProgress();RefreshHeaderWallet();RefreshSettings();
            SettingsNotice("Спасибо за подписку!","Получено: +"+amount+" монет.\nНаграда сохранена.");
        }
        private void RestoreFromSettings()
        {
            CloseSettings();Show(Page.Store);
            var store=GetComponentInChildren<AirtistStoreScreen>(true);if(store!=null)store.RestorePurchases();
        }
        private void OpenAdSettings()
        {
            OpenSettingsDetail("Реклама и данные","Вы можете изменить рекламные предпочтения. Видео за награду всегда остаётся добровольным.");
            DetailAction("Управление согласием",0,2,OpenConsentPreferences,AirtistApprovedTheme.Sage);
            DetailAction("Без рекламы",1,2,()=>{CloseSettings();Show(Page.Store);GetComponentInChildren<AirtistStoreScreen>(true)?.OpenProduct(0);});
        }
        private void OpenConsentPreferences()
        {
            if(settingsPrivacyBusy)return;
            if(!Application.isPlaying || Application.platform!=RuntimePlatform.Android || !MaxSdk.IsInitialized())
            {SettingsNotice("Рекламные предпочтения","Форма доступна на телефоне после загрузки рекламного сервиса. Попробуйте позднее.");return;}
            if(!MaxSdk.CmpService.HasSupportedCmp){SettingsNotice("Рекламные предпочтения","Форма согласия сейчас недоступна. Можно ознакомиться с политикой конфиденциальности.");return;}
            settingsPrivacyBusy=true;
            try
            {
                MaxSdk.CmpService.ShowCmpForExistingUser(error=>
                {
                    if(this==null)return;settingsPrivacyBusy=false;clockStamp=Time.realtimeSinceStartupAsDouble;
                    SettingsNotice("Рекламные предпочтения",error==null?"Настройки согласия сохранены.":error.Code==MaxCmpError.ErrorCode.FormNotRequired?"Для текущего региона форма сейчас не требуется.":"Форма сейчас недоступна. Попробуйте позднее.");
                });
            }
            catch(Exception){settingsPrivacyBusy=false;SettingsNotice("Рекламные предпочтения","Не удалось открыть форму. Попробуйте позднее.");}
        }
        private void OnDestroy()
        {
            if(settingsKnobSprite==null)return;
            var texture=settingsKnobSprite.texture;
            if(Application.isPlaying){Destroy(settingsKnobSprite);Destroy(texture);}
            else {DestroyImmediate(settingsKnobSprite);DestroyImmediate(texture);}
        }
    }
}
