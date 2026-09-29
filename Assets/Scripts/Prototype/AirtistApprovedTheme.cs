using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistApprovedTheme : ScriptableObject
    {
        public Sprite surface, homeBackdrop, museumBackdrop, amelie;
        public Sprite dailyGift, woodFrame, woodenMap;
        public Sprite settingsGear;
        public Sprite[] settingsIcons;
        public Sprite[] icons;
        public TMP_FontAsset font;
        public static readonly Color Paper=new Color(.99f,.96f,.88f), Ink=new Color(.035f,.19f,.23f),
            Coral=new Color(.96f,.52f,.36f), Sage=new Color(.67f,.79f,.52f), Teal=new Color(.16f,.46f,.49f),
            Honey=new Color(1f,.76f,.33f), Lavender=new Color(.73f,.61f,.88f);
        public static AirtistApprovedTheme Current => Resources.Load<AirtistApprovedTheme>("ApprovedTheme");
        public Sprite Icon(int n) => icons!=null && n>=0 && n<icons.Length?icons[n]:null;
        public Sprite SettingsIcon(int n) => settingsIcons!=null && n>=0 && n<settingsIcons.Length?settingsIcons[n]:null;
        public static void Rect(RectTransform r,float x,float y,float w,float h)
        {r.anchorMin=new Vector2(x,1-y-h);r.anchorMax=new Vector2(x+w,1-y);r.offsetMin=r.offsetMax=Vector2.zero;}
        public void Surface(UnityEngine.UI.Image i,Color tint)
        {if(i==null)return;i.sprite=surface;i.type=UnityEngine.UI.Image.Type.Sliced;i.pixelsPerUnitMultiplier=7;i.color=tint;}
        public void Wood(UnityEngine.UI.Image image)
        {
            if(image==null)return;
            if(woodFrame!=null)
            {
                var old=image.transform.Find("SlimWoodFrame");if(old!=null)old.gameObject.SetActive(false);
                var edge=Picture(image.transform,"OakFrame",woodFrame,0,0,1,1,false);
                edge.type=UnityEngine.UI.Image.Type.Sliced;edge.pixelsPerUnitMultiplier=7;
                edge.transform.SetAsLastSibling();return;
            }
            AirtistPaintingFrame.Attach(image);
            var frame=image.transform.Find("SlimWoodFrame")?.GetComponent<AirtistPaintingFrame>();
            if(frame!=null){frame.thickness=6;frame.color=new Color(.48f,.29f,.12f);frame.SetVerticesDirty();}
        }
        public void Button(UnityEngine.UI.Button b,Color tint)
        {
            if(b==null)return;Surface(b.image,tint);b.targetGraphic=b.image;b.image.raycastPadding=Vector4.zero;
            var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.99f,.96f);colors.pressedColor=new Color(.76f,.79f,.77f);colors.disabledColor=new Color(.7f,.7f,.7f,.65f);colors.fadeDuration=.10f;b.colors=colors;
            AirtistButtonFinish.Attach(b);
            AirtistMenuAudio.Wire(b);
            foreach(var raw in b.GetComponentsInChildren<UnityEngine.UI.RawImage>(true))raw.gameObject.SetActive(false);
            foreach(var t in b.GetComponentsInChildren<TMP_Text>(true))
            {t.raycastTarget=false;t.color=tint==Teal?Paper:Ink;t.fontStyle=FontStyles.Bold;}
        }
        public UnityEngine.UI.Image Picture(Transform parent,string name,Sprite sprite,float x,float y,float w,float h,bool preserve=true)
        {
            var child=parent.Find(name);var go=child!=null?child.gameObject:new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));
            if(child==null)go.transform.SetParent(parent,false);
            var image=go.GetComponent<UnityEngine.UI.Image>()??go.AddComponent<UnityEngine.UI.Image>();
            image.sprite=sprite;image.color=Color.white;image.preserveAspect=preserve;image.raycastTarget=false;
            Rect(image.rectTransform,x,y,w,h);return image;
        }
        public void Typography(TMP_Text text,RectTransform board,float size)
        {
            if(text==null)return;if(font!=null)text.font=font;text.color=Ink;text.raycastTarget=false;
            text.textWrappingMode=TextWrappingModes.Normal;text.overflowMode=TextOverflowModes.Ellipsis;
            var scale=text.GetComponent<AirtistScaledLabel>()??text.gameObject.AddComponent<AirtistScaledLabel>();
            scale.artboard=board;scale.designWidth=1672;scale.designFontSize=size;
        }
        public void Common(Transform root)
        {
            foreach(var b in root.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                if(b.name.StartsWith("MuseumPin") || b.name=="MuseumMapIcon" || b.name=="MuseumMapTile" || b.GetComponent<AirtistArtworkPointer>()!=null)continue;
                var c=b.image!=null?b.image.color:Paper;if(c.a<.2f)c=Paper;Button(b,c);
            }
        }
        public void Gameplay(AirtistGameplayScreen v)
        {
            var root=(RectTransform)v.transform;
            foreach(string n in new[]{"CharacterPanel","PaintingFrame","AmelieSpeechPanel","CharacterName","StatusDividerVertical","StatusDividerHorizontal","TimeIcon","ChecksIcon","TimeCaption","ChecksCaption"})Hide(root,n);
            Hide(root,"AmeliePortrait");Hide(root,"AmeliePortraitSlot");
            Picture(root,"ApprovedAmelie",amelie,.002f,.09f,.175f,.31f);
            var info=Picture(root,"ApprovedInfo",surface,.008f,.39f,.17f,.46f,false);Surface(info,Paper);Wood(info);info.transform.SetAsFirstSibling();
            var dock=root.Find("ToolDock")?.GetComponent<UnityEngine.UI.Image>();Surface(dock,Paper);if(dock!=null)Rect(dock.rectTransform,.185f,.855f,.815f,.145f);
            Rect(v.viewport,.185f,.112f,.805f,.733f);
            Rect(v.title.rectTransform,.027f,.409f,.132f,.075f);Typography(v.title,root,27);v.title.fontStyle=FontStyles.Bold;
            v.description.gameObject.SetActive(false);
            Rect(v.progress.rectTransform,.027f,.683f,.132f,.065f);Typography(v.progress,root,27);
            v.progress.fontStyle=FontStyles.Bold;
            Rect(v.feedback.rectTransform,.027f,.753f,.132f,.06f);Typography(v.feedback,root,16);
            Rect(v.hint.rectTransform,.025f,.795f,.18f,.001f);v.hint.gameObject.SetActive(false);
            var hud=v.GetComponent<AirtistAttemptHud>();
            if(hud!=null)
            {
                Hide(root,"AttemptStatusPanel");Rect(hud.timeLabel.rectTransform,.024f,.62f,.075f,.04f);Rect(hud.clicksLabel.rectTransform,.114f,.62f,.078f,.04f);
                hud.separateCaptions=false;
                Rect(hud.timeLabel.rectTransform,.027f,.505f,.132f,.07f);Rect(hud.clicksLabel.rectTransform,.027f,.595f,.132f,.07f);
                Typography(hud.timeLabel,root,27);Typography(hud.clicksLabel,root,25);
                hud.timeLabel.fontStyle=FontStyles.Bold;hud.clicksLabel.fontStyle=FontStyles.Bold;
                var dialog=hud.endMessage.transform.parent as RectTransform;
                Rect((RectTransform)hud.endPanel.transform,0,.08f,1,.92f);
                Rect(dialog,.25f,.12f,.50f,.70f);Surface(dialog.GetComponent<UnityEngine.UI.Image>(),Paper);
                Rect(hud.endMessage.rectTransform,.09f,.10f,.82f,.40f);Typography(hud.endMessage,root,25);
                Rect((RectTransform)hud.rewardedContinue.transform,.055f,.53f,.435f,.17f);Button(hud.rewardedContinue,Lavender);
                Rect((RectTransform)hud.bonusContinue.transform,.51f,.53f,.435f,.17f);Button(hud.bonusContinue,Honey);
                Rect((RectTransform)hud.restart.transform,.055f,.74f,.435f,.17f);Button(hud.restart,Paper);
                Rect((RectTransform)hud.home.transform,.51f,.74f,.435f,.17f);Button(hud.home,Teal);
                if(hud.close!=null)Rect((RectTransform)hud.close.transform,.88f,.02f,.10f,.10f);
                hud.endPanel.transform.SetAsLastSibling();
            }
            var buttons=new[]{v.mainTool,v.markTool,v.zoomTool,v.brushes[0],v.brushes[1],v.brushes[2],v.brushes[3]};
            string[] names={"Проверка","Пометка","Обзор","Область","Точно","Убрать","Видео"};
            Color[] colors={Teal,Paper,Paper,Honey,Sage,Coral,Lavender};
            float[] x={.196f,.299f,.402f,.524f,.621f,.718f,.815f};
            for(int i=0;i<7;i++)
            {
                var b=buttons[i];Rect((RectTransform)b.transform,x[i],.868f,i<3?.095f:.089f,.12f);Button(b,colors[i]);
                var old=b.GetComponentInChildren<AirtistToolIcon>(true);if(old!=null)old.gameObject.SetActive(false);
                Picture(b.transform,"PaintedIcon",Icon(i),.15f,.04f,.70f,i<3?.62f:.49f);
                var label=b.GetComponentInChildren<TMP_Text>();label.text=names[i];Rect(label.rectTransform,.02f,.67f,.96f,.29f);Typography(label,root,21);label.alignment=TextAlignmentOptions.Center;
                if(i==0)label.color=Paper;
                if(i>=3 && v.brushCosts!=null && v.brushCosts.Length>i-3)
                {
                    var cost=v.brushCosts[i-3];Rect(cost.rectTransform,.05f,.48f,.90f,.20f);Typography(cost,root,17);cost.alignment=TextAlignmentOptions.Center;cost.fontStyle=FontStyles.Bold;cost.transform.SetAsLastSibling();
                }
            }
            Rect((RectTransform)v.help.transform,.925f,.89f,.060f,.085f);Button(v.help,Paper);
            var stock=root.Find("HintStock")?.GetComponent<TMP_Text>();
            if(stock==null){var go=new GameObject("HintStock",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(root,false);stock=go.GetComponent<TMP_Text>();}
            var stockPaper=Picture(root,"HintStockPaper",surface,.008f,.865f,.17f,.12f,false);
            Surface(stockPaper,Paper);stockPaper.transform.SetAsFirstSibling();
            stock.text="Подсказки: 3";Rect(stock.rectTransform,.018f,.88f,.15f,.045f);Typography(stock,root,22);stock.alignment=TextAlignmentOptions.Center;
            var caption=root.Find("HintStockCaption")?.GetComponent<TMP_Text>();
            if(caption==null){var go=new GameObject("HintStockCaption",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(root,false);caption=go.GetComponent<TMP_Text>();}
            caption.text="Для бонусных кистей";Rect(caption.rectTransform,.018f,.934f,.15f,.032f);Typography(caption,root,16);caption.alignment=TextAlignmentOptions.Center;
            if(hud!=null)hud.endPanel.transform.SetAsLastSibling();
        }
        public void Gallery(AirtistMyGalleryScreen view)
        {
            var root=(RectTransform)view.transform;Common(root);
            var profile=root.Find("ProfilePanel") as RectTransform;if(profile!=null){Rect(profile,.012f,.49f,.195f,.30f);Surface(profile.GetComponent<UnityEngine.UI.Image>(),Paper);Wood(profile.GetComponent<UnityEngine.UI.Image>());Hide(profile,"Avatar");}
            if(profile!=null)
            {
                string[] n={"PlayerName","PlayerRank","PaintingCount","PaintingCaption","TraceCount","TraceCaption","MuseumCount","MuseumCaption"};
                for(int i=0;i<n.Length;i++)
                {
                    var t=profile.Find(n[i])?.GetComponent<TMP_Text>();if(t==null)continue;
                    if(i<2)Rect(t.rectTransform,.10f,.095f+i*.16f,.80f,.14f);
                    else{int row=(i-2)/2;Rect(t.rectTransform,i%2==0?.12f:.34f,.42f+row*.16f,i%2==0?.18f:.54f,.14f);}
                    Typography(t,root,i==0?29:i==1?18:22);t.alignment=TextAlignmentOptions.Left;
                }
            }
            var portrait=Picture(root,"ApprovedAmelie",amelie,0,.11f,.205f,.40f);
            portrait.rectTransform.pivot=new Vector2(.5f,0);
            var board=Picture(root,"ApprovedGalleryPaper",surface,.25f,.095f,.74f,.87f,false);Surface(board,Paper);board.transform.SetAsFirstSibling();
            var title=root.Find("ScreenTitle")?.GetComponent<TMP_Text>();if(title!=null){Rect(title.rectTransform,.27f,.115f,.69f,.065f);Typography(title,root,40);}
            var tabs=root.Find("MuseumFilters") as RectTransform;if(tabs!=null)Rect(tabs,.265f,.197f,.715f,.075f);
            for(int i=0;i<view.museumFilters.Length;i++)
            {var b=view.museumFilters[i];Rect((RectTransform)b.transform,i*.20f,0,.19f,1);Button(b,i==0?Teal:Paper);foreach(var t in b.GetComponentsInChildren<TMP_Text>())Typography(t,root,22);}
            Rect(view.sectionTitle.rectTransform,.27f,.275f,.69f,.035f);Typography(view.sectionTitle,root,20);
            Rect((RectTransform)view.scroll.transform,.266f,.315f,.715f,.565f);
            Rect((RectTransform)view.nextPainting.transform,.38f,.885f,.45f,.09f);Button(view.nextPainting,Coral);
            var nextLabel=view.nextPainting.GetComponentInChildren<TMP_Text>();
            Typography(nextLabel,root,30);nextLabel.fontStyle=FontStyles.Bold;
            Rect(nextLabel.rectTransform,.06f,.10f,.80f,.80f);
            var arrow=view.nextPainting.transform.Find("NextArrow")?.GetComponent<TMP_Text>();
            if(arrow==null){var go=new GameObject("NextArrow",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(view.nextPainting.transform,false);arrow=go.GetComponent<TMP_Text>();}
            arrow.text="›";Rect(arrow.rectTransform,.88f,.07f,.07f,.86f);Typography(arrow,root,42);arrow.alignment=TextAlignmentOptions.Center;
            view.content.GetComponent<AirtistGalleryGrid>().cardHeightToWidth=1.05f;
        }
        public static void Hide(Transform root,string name){var t=root.Find(name);if(t!=null)t.gameObject.SetActive(false);}
        public void Home(AirtistHomeScreen view)
        {
            var root=(RectTransform)view.transform;
            foreach(Transform t in root)
                if(t.name.StartsWith("Artwork_") || t.name.StartsWith("Flexible") || t.name.StartsWith("Text_nav") || t.name=="Text_home.location" || t.name.StartsWith("Text_home.character"))t.gameObject.SetActive(false);
            for(int i=0;i<view.Buttons.Length;i++)view.Buttons[i].gameObject.SetActive(i==5||i==6);
            foreach(string n in new[]{"Text_home.route.caption","Text_home.daily.description","Text_home.map.open"})Hide(root,n);
            Picture(root,"ApprovedAmelie",amelie,.59f,.13f,.32f,.65f);
            var panel=Picture(root,"ApprovedRoute",surface,.025f,.49f,.445f,.275f,false);Surface(panel,Paper);Wood(panel);panel.transform.SetAsFirstSibling();
            var title=root.Find("Text_home.title.old")?.GetComponent<TMP_Text>();
            var modern=root.Find("Text_home.title.modern")?.GetComponent<TMP_Text>();
            if(title!=null)title.gameObject.SetActive(false);
            if(modern!=null)modern.gameObject.SetActive(false);
            Hide(root,"Text_home.subtitle");
            PlaceText(root.Find("Text_home.route.title")?.GetComponent<TMP_Text>(),.245f,.54f,.21f,.065f,38);
            PlaceText(root.Find("Text_home.route.summary")?.GetComponent<TMP_Text>(),.245f,.635f,.21f,.095f,23);
            var route=Picture(root,"ApprovedRouteThumbnail",null,.045f,.515f,.175f,.22f);route.transform.SetSiblingIndex(panel.transform.GetSiblingIndex()+1);
            Rect((RectTransform)view.Buttons[5].transform,.025f,.78f,.335f,.16f);Button(view.Buttons[5],Coral);
            Rect((RectTransform)view.Buttons[6].transform,.367f,.78f,.215f,.16f);Button(view.Buttons[6],Honey);
            Picture(view.Buttons[5].transform,"PaintedIcon",Icon(0),.04f,.12f,.23f,.76f);
            Picture(view.Buttons[6].transform,"PaintedIcon",dailyGift!=null?dailyGift:Icon(7),.05f,.08f,.32f,.84f);
            PlaceText(root.Find("Text_home.continue")?.GetComponent<TMP_Text>(),.12f,.813f,.225f,.095f,34);
            var continueText=root.Find("Text_home.continue")?.GetComponent<TMP_Text>();if(continueText!=null)continueText.color=Paper;
            var claim=root.Find("Text_home.daily.claim")?.GetComponent<TMP_Text>();PlaceText(claim,.445f,.806f,.125f,.11f,28);
            void PlaceText(TMP_Text t,float x,float y,float w,float h,float size){if(t==null)return;Rect(t.rectTransform,x,y,w,h);Typography(t,root,size);t.transform.SetAsLastSibling();}
        }
        public void Store(AirtistStoreScreen view)
        {
            var root=(RectTransform)view.transform;Common(root);
            Hide(root,"GoldPanel/SectionTitle");Hide(root,"HintsPanel/SectionTitle");
            var panel=Picture(root,"ApprovedStorePaper",surface,.245f,.095f,.735f,.875f,false);Surface(panel,Paper);panel.transform.SetAsFirstSibling();
            Picture(root,"ApprovedAmelie",amelie,0,.12f,.225f,.70f);
            var title=root.Find("StoreTitle")?.GetComponent<TMP_Text>();if(title!=null){Rect(title.rectTransform,.30f,.11f,.60f,.075f);Typography(title,root,42);}
            var noads=view.offers[0];Rect((RectTransform)noads.transform,.26f,.205f,.21f,.66f);Surface(noads.GetComponent<UnityEngine.UI.Image>(),Sage);
            foreach(string n in new[]{"Benefit","Description","PurchaseType","RewardedNote"})
            {var t=noads.transform.Find(n)?.GetComponent<TMP_Text>();if(t!=null){Typography(t,root,22);t.fontStyle=FontStyles.Bold;}}
            Rect(noads.title.rectTransform,.10f,.045f,.80f,.08f);
            var rewardedNote=noads.transform.Find("RewardedNote")?.GetComponent<TMP_Text>();
            if(rewardedNote!=null){Rect(rewardedNote.rectTransform,.10f,.89f,.80f,.065f);Typography(rewardedNote,root,18);}
            for(int i=1;i<view.offers.Length;i++)
            {
                var o=view.offers[i];o.transform.SetParent(root,false);
                Rect((RectTransform)o.transform,.477f+((i-1)%3)*.162f,i<4?.205f:.54f,.155f,.325f);
                Surface(o.GetComponent<UnityEngine.UI.Image>(),Paper);
                Rect(o.title.rectTransform,.10f,.09f,.80f,.15f);Typography(o.title,root,24);
                Rect(o.icon.rectTransform,.16f,.27f,.68f,.42f);
                Rect((RectTransform)o.buy.transform,.10f,.735f,.80f,.175f);Button(o.buy,Sage);
                Typography(o.price,root,26);
            }
            foreach(string n in new[]{"GoldPanel","HintsPanel"})Hide(root,n);
            var restore=root.Find("RestorePurchases") as RectTransform;if(restore!=null){Rect(restore,.70f,.905f,.265f,.06f);Button(restore.GetComponent<UnityEngine.UI.Button>(),Paper);}
            var notice=root.Find("DemoPriceNotice") as RectTransform;if(notice!=null)Rect(notice,.26f,.90f,.425f,.07f);
            var dialog=view.dialogTitle.transform.parent;Surface(dialog.GetComponent<UnityEngine.UI.Image>(),Paper);
            Wood(dialog.GetComponent<UnityEngine.UI.Image>());
            foreach(var offer in view.offers)Wood(offer.GetComponent<UnityEngine.UI.Image>());
            var dialogRect=(RectTransform)dialog;
            Rect(dialogRect,.20f,.18f,.60f,.70f);
            Rect(view.dialogTitle.rectTransform,.08f,.075f,.78f,.12f);Typography(view.dialogTitle,root,36);view.dialogTitle.fontStyle=FontStyles.Bold;
            view.dialogIcon=Picture(dialog,"ProductIcon",view.offers[0].icon.sprite,.07f,.23f,.27f,.43f);
            Rect(view.dialogBody.rectTransform,.39f,.235f,.54f,.36f);Typography(view.dialogBody,root,24);view.dialogBody.alignment=TextAlignmentOptions.Left;
            Rect(view.dialogPrice.rectTransform,.39f,.61f,.54f,.075f);Typography(view.dialogPrice,root,24);view.dialogPrice.fontStyle=FontStyles.Bold;
            Rect((RectTransform)view.confirm.transform,.07f,.77f,.42f,.15f);Button(view.confirm,Coral);
            var close=dialog.Find("CloseDialog")?.GetComponent<UnityEngine.UI.Button>();
            if(close!=null){Rect((RectTransform)close.transform,.52f,.77f,.41f,.15f);Button(close,Paper);}
            foreach(var b in new[]{view.confirm,close})if(b!=null){var text=b.GetComponentInChildren<TMP_Text>();Rect(text.rectTransform,.06f,.10f,.88f,.80f);Typography(text,root,26);}
            var closeTile=Picture(dialog,"CloseTile",surface,.895f,.055f,.075f,.115f,false);
            Surface(closeTile,Coral);closeTile.raycastTarget=true;
            var closeButton=closeTile.GetComponent<UnityEngine.UI.Button>()??closeTile.gameObject.AddComponent<UnityEngine.UI.Button>();
            closeButton.targetGraphic=closeTile;closeButton.onClick.RemoveAllListeners();closeButton.onClick.AddListener(view.CloseDialog);
            var cross=closeTile.transform.Find("Cross")?.GetComponent<TMP_Text>();
            if(cross==null){var go=new GameObject("Cross",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(closeTile.transform,false);cross=go.GetComponent<TMP_Text>();}
            cross.text="×";Rect(cross.rectTransform,0,0,1,1);Typography(cross,root,38);cross.alignment=TextAlignmentOptions.Center;
            view.dialog.transform.SetAsLastSibling();
        }
        public void Card(AirtistGalleryCard card)
        {
            Surface(card.GetComponent<UnityEngine.UI.Image>(),Paper);
            Button(card.action,!card.action.interactable?Paper:card.state.text=="В коллекции"?Sage:Coral);
            foreach(string n in new[]{"GoldFrame","PaperMat","CaptionPanel"})Hide(card.transform,n);
            Rect(card.artwork.rectTransform,.04f,.025f,.92f,.57f);card.artwork.preserveAspect=true;
            Rect(card.title.rectTransform,.04f,.605f,.92f,.09f);Rect(card.artist.rectTransform,.04f,.698f,.92f,.064f);
            Rect(card.state.rectTransform,.04f,.765f,.92f,.065f);Rect((RectTransform)card.action.transform,.04f,.845f,.92f,.125f);
            if(card.locked!=null)
            {
                foreach(var oldText in card.locked.GetComponentsInChildren<TMP_Text>(true))oldText.enabled=false;
                foreach(var oldIcon in card.locked.GetComponentsInChildren<AirtistToolIcon>(true))oldIcon.enabled=false;
                var lockImage=card.locked.GetComponent<UnityEngine.UI.Image>()??card.locked.AddComponent<UnityEngine.UI.Image>();
                lockImage.sprite=Icon(15);lockImage.type=UnityEngine.UI.Image.Type.Simple;
                lockImage.color=Color.white;lockImage.preserveAspect=true;lockImage.raycastTarget=false;
                Rect(lockImage.rectTransform,.40f,.215f,.20f,.20f);card.locked.transform.SetAsLastSibling();
            }
            var row=card.transform.Find("RatingStars") as RectTransform;if(row!=null)Rect(row,.10f,.763f,.43f,.075f);
            foreach(var t in card.GetComponentsInChildren<TMP_Text>(true)){var s=t.GetComponent<AirtistScaledLabel>();if(s!=null){s.artboard=(RectTransform)card.transform;s.designWidth=380;s.designFontSize=t==card.title?24:t==card.artist?18:21;}t.font=font;t.color=Ink;}
        }
    }
}
