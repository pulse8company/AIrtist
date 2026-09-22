using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistApprovedTheme : ScriptableObject
    {
        public Sprite surface, homeBackdrop, museumBackdrop, amelie;
        public Sprite[] icons;
        public TMP_FontAsset font;
        public static readonly Color Paper=new Color(.99f,.96f,.88f), Ink=new Color(.035f,.19f,.23f),
            Coral=new Color(.96f,.52f,.36f), Sage=new Color(.67f,.79f,.52f), Teal=new Color(.16f,.46f,.49f),
            Honey=new Color(1f,.76f,.33f), Lavender=new Color(.73f,.61f,.88f);
        public static AirtistApprovedTheme Current => Resources.Load<AirtistApprovedTheme>("ApprovedTheme");
        public Sprite Icon(int n) => icons!=null && n>=0 && n<icons.Length?icons[n]:null;
        public static void Rect(RectTransform r,float x,float y,float w,float h)
        {r.anchorMin=new Vector2(x,1-y-h);r.anchorMax=new Vector2(x+w,1-y);r.offsetMin=r.offsetMax=Vector2.zero;}
        public void Surface(UnityEngine.UI.Image i,Color tint)
        {if(i==null)return;i.sprite=surface;i.type=UnityEngine.UI.Image.Type.Sliced;i.pixelsPerUnitMultiplier=7;i.color=tint;}
        public void Button(UnityEngine.UI.Button b,Color tint)
        {
            if(b==null)return;Surface(b.image,tint);b.targetGraphic=b.image;b.image.raycastPadding=Vector4.zero;
            var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.98f,.94f);colors.pressedColor=new Color(.8f,.8f,.8f);colors.disabledColor=new Color(.7f,.7f,.7f,.65f);b.colors=colors;
            foreach(var raw in b.GetComponentsInChildren<UnityEngine.UI.RawImage>(true))raw.gameObject.SetActive(false);
            foreach(var t in b.GetComponentsInChildren<TMP_Text>(true))t.raycastTarget=false;
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
                if(b.name.StartsWith("MuseumPin") || b.name=="MuseumMapIcon" || b.GetComponent<AirtistArtworkPointer>()!=null)continue;
                var c=b.image!=null?b.image.color:Paper;if(c.a<.2f)c=Paper;Button(b,c);
            }
        }
        public void Gameplay(AirtistGameplayScreen v)
        {
            var root=(RectTransform)v.transform;
            foreach(string n in new[]{"CharacterPanel","PaintingFrame","AmelieSpeechPanel","CharacterName","StatusDividerVertical","StatusDividerHorizontal","TimeIcon","ChecksIcon","TimeCaption","ChecksCaption"})Hide(root,n);
            Hide(root,"AmeliePortrait");Hide(root,"AmeliePortraitSlot");
            Picture(root,"ApprovedAmelie",amelie,.002f,.11f,.20f,.40f);
            var info=Picture(root,"ApprovedInfo",surface,.008f,.485f,.196f,.305f,false);Surface(info,Paper);info.transform.SetAsFirstSibling();
            var dock=root.Find("ToolDock")?.GetComponent<UnityEngine.UI.Image>();Surface(dock,Paper);if(dock!=null)Rect(dock.rectTransform,0,.797f,1,.203f);
            Rect(v.viewport,.215f,.085f,.775f,.705f);
            Rect(v.title.rectTransform,.024f,.50f,.168f,.065f);Typography(v.title,root,29);
            Rect(v.description.rectTransform,.024f,.568f,.168f,.045f);Typography(v.description,root,20);
            v.description.gameObject.SetActive(true);
            Rect(v.progress.rectTransform,.024f,.668f,.168f,.04f);Typography(v.progress,root,23);
            Rect(v.feedback.rectTransform,.024f,.713f,.168f,.07f);Typography(v.feedback,root,18);
            Rect(v.hint.rectTransform,.025f,.795f,.18f,.001f);v.hint.gameObject.SetActive(false);
            var hud=v.GetComponent<AirtistAttemptHud>();
            if(hud!=null)
            {
                Hide(root,"AttemptStatusPanel");Rect(hud.timeLabel.rectTransform,.024f,.62f,.075f,.04f);Rect(hud.clicksLabel.rectTransform,.114f,.62f,.078f,.04f);
                hud.separateCaptions=false;
                Rect(hud.timeLabel.rectTransform,.024f,.61f,.075f,.052f);Rect(hud.clicksLabel.rectTransform,.114f,.61f,.078f,.052f);
                Typography(hud.timeLabel,root,19);Typography(hud.clicksLabel,root,19);
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
            float[] x={.024f,.153f,.278f,.464f,.576f,.688f,.800f};
            for(int i=0;i<7;i++)
            {
                var b=buttons[i];Rect((RectTransform)b.transform,x[i],.805f,i<3?.115f:.106f,.177f);Button(b,colors[i]);
                var old=b.GetComponentInChildren<AirtistToolIcon>(true);if(old!=null)old.gameObject.SetActive(false);
                Picture(b.transform,"PaintedIcon",Icon(i),.15f,.04f,.70f,.62f);
                var label=b.GetComponentInChildren<TMP_Text>();label.text=names[i];Rect(label.rectTransform,.04f,.69f,.92f,.24f);Typography(label,root,24);label.alignment=TextAlignmentOptions.Center;
                if(i==0)label.color=Paper;
                if(i>=3 && v.brushCosts!=null && v.brushCosts.Length>i-3)
                {
                    var cost=v.brushCosts[i-3];Rect(cost.rectTransform,.67f,.035f,.28f,.22f);Typography(cost,root,19);cost.transform.SetAsLastSibling();
                }
            }
            Rect((RectTransform)v.help.transform,.924f,.864f,.060f,.105f);Button(v.help,Paper);
            var stock=root.Find("HintStock")?.GetComponent<TMP_Text>();
            if(stock==null){var go=new GameObject("HintStock",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(root,false);stock=go.GetComponent<TMP_Text>();}
            stock.text="Запас:\n3";Rect(stock.rectTransform,.399f,.857f,.059f,.088f);Typography(stock,root,20);stock.alignment=TextAlignmentOptions.Center;
            if(hud!=null)hud.endPanel.transform.SetAsLastSibling();
        }
        public void Gallery(AirtistMyGalleryScreen view)
        {
            var root=(RectTransform)view.transform;Common(root);
            var profile=root.Find("ProfilePanel") as RectTransform;if(profile!=null){Rect(profile,.012f,.49f,.195f,.30f);Surface(profile.GetComponent<UnityEngine.UI.Image>(),Paper);Hide(profile,"Avatar");}
            if(profile!=null)
            {
                string[] n={"PlayerName","PlayerRank","PaintingCount","PaintingCaption","TraceCount","TraceCaption","MuseumCount","MuseumCaption"};
                for(int i=0;i<n.Length;i++)
                {
                    var t=profile.Find(n[i])?.GetComponent<TMP_Text>();if(t==null)continue;
                    if(i<2)Rect(t.rectTransform,.08f,.06f+i*.16f,.84f,.14f);
                    else{int row=(i-2)/2;Rect(t.rectTransform,i%2==0?.10f:.32f,.40f+row*.18f,i%2==0?.18f:.60f,.15f);}
                    Typography(t,root,i==0?29:i==1?18:22);t.alignment=TextAlignmentOptions.Left;
                }
            }
            Picture(root,"ApprovedAmelie",amelie,0,.11f,.205f,.40f);
            var board=Picture(root,"ApprovedGalleryPaper",surface,.25f,.095f,.74f,.87f,false);Surface(board,Paper);board.transform.SetAsFirstSibling();
            var title=root.Find("ScreenTitle")?.GetComponent<TMP_Text>();if(title!=null){Rect(title.rectTransform,.27f,.105f,.69f,.075f);Typography(title,root,40);}
            var tabs=root.Find("MuseumFilters") as RectTransform;if(tabs!=null)Rect(tabs,.265f,.197f,.715f,.075f);
            for(int i=0;i<view.museumFilters.Length;i++)
            {var b=view.museumFilters[i];Rect((RectTransform)b.transform,i*.20f,0,.19f,1);Button(b,i==0?Teal:Paper);foreach(var t in b.GetComponentsInChildren<TMP_Text>())Typography(t,root,22);}
            Rect(view.sectionTitle.rectTransform,.27f,.275f,.69f,.035f);Typography(view.sectionTitle,root,20);
            Rect((RectTransform)view.scroll.transform,.266f,.315f,.715f,.565f);
            Rect((RectTransform)view.nextPainting.transform,.38f,.885f,.45f,.09f);Button(view.nextPainting,Coral);
            Typography(view.nextPainting.GetComponentInChildren<TMP_Text>(),root,30);
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
            var panel=Picture(root,"ApprovedRoute",surface,.025f,.49f,.445f,.275f,false);Surface(panel,Paper);panel.transform.SetAsFirstSibling();
            var title=root.Find("Text_home.title.old")?.GetComponent<TMP_Text>();
            var modern=root.Find("Text_home.title.modern")?.GetComponent<TMP_Text>();
            PlaceText(title,.12f,.175f,.47f,.115f,66);PlaceText(modern,.12f,.28f,.47f,.115f,66);
            PlaceText(root.Find("Text_home.subtitle")?.GetComponent<TMP_Text>(),.13f,.385f,.44f,.10f,26);
            PlaceText(root.Find("Text_home.route.title")?.GetComponent<TMP_Text>(),.245f,.54f,.21f,.065f,38);
            PlaceText(root.Find("Text_home.route.summary")?.GetComponent<TMP_Text>(),.245f,.635f,.21f,.095f,23);
            var route=Picture(root,"ApprovedRouteThumbnail",null,.045f,.515f,.175f,.22f);route.transform.SetSiblingIndex(panel.transform.GetSiblingIndex()+1);
            Rect((RectTransform)view.Buttons[5].transform,.025f,.78f,.335f,.16f);Button(view.Buttons[5],Coral);
            Rect((RectTransform)view.Buttons[6].transform,.367f,.78f,.215f,.16f);Button(view.Buttons[6],Honey);
            Picture(view.Buttons[5].transform,"PaintedIcon",Icon(0),.04f,.12f,.23f,.76f);
            Picture(view.Buttons[6].transform,"PaintedIcon",Icon(7),.07f,.16f,.28f,.68f);
            PlaceText(root.Find("Text_home.continue")?.GetComponent<TMP_Text>(),.12f,.825f,.225f,.075f,30);
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
            {var t=noads.transform.Find(n)?.GetComponent<TMP_Text>();if(t!=null)Typography(t,root,18);}
            for(int i=1;i<view.offers.Length;i++)
            {
                var o=view.offers[i];o.transform.SetParent(root,false);
                Rect((RectTransform)o.transform,.477f+((i-1)%3)*.162f,i<4?.205f:.54f,.155f,.325f);
                Surface(o.GetComponent<UnityEngine.UI.Image>(),Paper);
                Rect(o.title.rectTransform,.035f,.04f,.93f,.16f);Typography(o.title,root,24);
                Rect(o.icon.rectTransform,.12f,.23f,.76f,.47f);
                Rect((RectTransform)o.buy.transform,.07f,.755f,.86f,.205f);Button(o.buy,Sage);
                Typography(o.price,root,26);
            }
            foreach(string n in new[]{"GoldPanel","HintsPanel"})Hide(root,n);
            var restore=root.Find("RestorePurchases") as RectTransform;if(restore!=null){Rect(restore,.70f,.905f,.265f,.06f);Button(restore.GetComponent<UnityEngine.UI.Button>(),Paper);}
            var notice=root.Find("DemoPriceNotice") as RectTransform;if(notice!=null)Rect(notice,.26f,.90f,.425f,.07f);
            var dialog=view.dialogTitle.transform.parent;Surface(dialog.GetComponent<UnityEngine.UI.Image>(),Paper);
            view.dialog.transform.SetAsLastSibling();
        }
        public void Card(AirtistGalleryCard card)
        {
            Surface(card.GetComponent<UnityEngine.UI.Image>(),Paper);Button(card.action,Sage);
            foreach(string n in new[]{"GoldFrame","PaperMat","CaptionPanel"})Hide(card.transform,n);
            Rect(card.artwork.rectTransform,.04f,.025f,.92f,.57f);card.artwork.preserveAspect=true;
            Rect(card.title.rectTransform,.04f,.605f,.92f,.09f);Rect(card.artist.rectTransform,.04f,.698f,.92f,.064f);
            Rect(card.state.rectTransform,.04f,.765f,.92f,.065f);Rect((RectTransform)card.action.transform,.04f,.845f,.92f,.125f);
            var row=card.transform.Find("RatingStars") as RectTransform;if(row!=null)Rect(row,.10f,.763f,.43f,.075f);
            foreach(var t in card.GetComponentsInChildren<TMP_Text>(true)){var s=t.GetComponent<AirtistScaledLabel>();if(s!=null){s.artboard=(RectTransform)card.transform;s.designWidth=380;s.designFontSize=t==card.title?24:t==card.artist?18:21;}t.font=font;t.color=Ink;}
        }
    }
}
