using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistGameplayBuilder
    {
        public const string Path = "Assets/UI/Gameplay/GameplayScreen.prefab";
        private static RectTransform root;
        private static Sprite panel;
        private static Texture2D reference;
        private static TMP_FontAsset font;
        private static readonly Color Ink = new Color(.035f,.19f,.23f);

        public static string CreateAndConnect()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            var controller = UnityEngine.Object.FindFirstObjectByType<AirtistLandscapePrototypeController>();
            if (controller == null || controller.gameObject.scene.path != "Assets/Scenes/AirtistLandscapePrototype.unity")
                throw new InvalidOperationException("Open the AIrtist landscape scene.");
            if (!AssetDatabase.IsValidFolder("Assets/UI/Gameplay")) AssetDatabase.CreateFolder("Assets/UI", "Gameplay");
            panel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/MyGallery/GalleryPanel.png");
            reference = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Prototype/Gameplay/GameplayReference.png");
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AirtistLocalizationBuilder.FontPath);
            if (panel == null || reference == null || font == null) throw new InvalidOperationException("Gameplay art/font missing.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Path) == null) Create();
            UpgradeAttemptHud();
            ApplyStatusStyle();
            Connect(controller);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            EditorSceneManager.SaveScene(controller.gameObject.scene);
            AssetDatabase.SaveAssets();
            return "Gameplay prefab and attempt rules connected: four resource counters, check/mark tools, failure dialog. No Play/build.";
        }

        public static void Connect(AirtistLandscapePrototypeController controller)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (prefab != null) controller.ConfigureGameplay(prefab.GetComponent<AirtistGameplayScreen>());
            var rules=AssetDatabase.LoadAssetAtPath<AirtistAttemptRules>("Assets/UI/Gameplay/AttemptRules.asset");
            if(rules!=null) controller.ConfigureAttemptRules(rules);
        }

        private static void UpgradeAttemptHud()
        {
            const string rulesPath="Assets/UI/Gameplay/AttemptRules.asset";
            if(AssetDatabase.LoadAssetAtPath<AirtistAttemptRules>(rulesPath)==null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AirtistAttemptRules>(),rulesPath);
            var prefab=PrefabUtility.LoadPrefabContents(Path);
            try
            {
                root=prefab.GetComponent<RectTransform>();
                if(prefab.GetComponent<AirtistAttemptHud>()!=null) return;
                var view=prefab.GetComponent<AirtistGameplayScreen>();
                var hud=prefab.AddComponent<AirtistAttemptHud>();
                Box(root,"AttemptStatusPanel",14,119,306,101,Color.white);
                hud.timeLabel=Label(root,"TimeRemaining","Время  —",18,26,132,142,32);
                hud.clicksLabel=Label(root,"ChecksRemaining","Проверки  12",18,173,132,136,32);
                hud.energyLabel=Label(root,"EnergyBalance","Энергия  100/100",17,26,178,155,28);
                hud.coinsLabel=Label(root,"CoinBalance","Монеты  0",17,184,178,125,28);
                var portrait=root.Find("AmeliePortrait") as RectTransform;
                if(portrait!=null) Place(portrait,94,228,145,260);

                // Keep the painting and bonus slots in place; split only the existing tool area.
                Place((RectTransform)view.mainTool.transform,390,831,128,97);
                var oldIcon=view.mainTool.transform.Find("Magnifier");
                if(oldIcon!=null) oldIcon.gameObject.SetActive(false);
                var text=view.mainTool.GetComponentInChildren<TextMeshProUGUI>(); text.text="Проверка";
                text.rectTransform.anchorMin=new Vector2(.04f,.08f); text.rectTransform.anchorMax=new Vector2(.96f,.92f);
                text.rectTransform.offsetMin=text.rectTransform.offsetMax=Vector2.zero;
                text.GetComponent<AirtistScaledLabel>().designFontSize=21;
                view.markTool=Button("MarkTool","Пометка",530,831,128,97,Color.white);
                view.markTool.GetComponentInChildren<AirtistScaledLabel>().designFontSize=21;
                view.zoomTool=Button("ZoomTool","Зум",1290,839,110,82,Color.white);
                view.zoomTool.GetComponentInChildren<AirtistScaledLabel>().designFontSize=22;

                var shade=Box(root,"AttemptEnded",338,110,1334,710,new Color(.16f,.25f,.26f,.97f));
                shade.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
                var dialog=Box(shade,"Dialog",122,120,1090,475,Color.white);
                hud.endMessage=Label(dialog,"Reason","Проверки закончились.",27,40,25,1010,175);
                hud.rewardedContinue=ChildButton(dialog,"RewardedContinue","Продолжить за видео",40,225,490,76);
                hud.bonusContinue=ChildButton(dialog,"BonusContinue","Бонус продолжения · 0",560,225,490,76);
                hud.restart=ChildButton(dialog,"RestartAttempt","Новая попытка",40,345,490,76);
                hud.home=ChildButton(dialog,"ReturnHome","На главную",560,345,490,76);
                hud.rewardedContinue.interactable=false; hud.bonusContinue.interactable=false;
                hud.endPanel=shade.gameObject; shade.gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(prefab,Path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }

        private static void Place(RectTransform rect,float x,float y,float w,float h)
        {
            rect.anchorMin=new Vector2(x/1672,1-(y+h)/941); rect.anchorMax=new Vector2((x+w)/1672,1-y/941);
            rect.offsetMin=rect.offsetMax=Vector2.zero;
        }

        public static string FixAttemptDialog()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            var prefab=PrefabUtility.LoadPrefabContents(Path);
            try
            {
                root=(RectTransform)prefab.transform;
                panel=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/MyGallery/GalleryPanel.png");
                font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AirtistLocalizationBuilder.FontPath);
                var hud=prefab.GetComponent<AirtistAttemptHud>();
                var dialog=(RectTransform)hud.endMessage.transform.parent;
                if(hud.close==null) hud.close=ChildButton(dialog,"CloseDialog","×",946,35,84,76);
                FitDialogRect(hud.endMessage.rectTransform,90,55,820,170);
                hud.endMessage.text="Время закончилось.\nПродолжить — сохранить находки.\nЗаново — искать с начала.\nВидео пока недоступно.";
                FitDialogRect((RectTransform)hud.rewardedContinue.transform,90,240,440,76);
                FitDialogRect((RectTransform)hud.bonusContinue.transform,560,240,440,76);
                FitDialogRect((RectTransform)hud.restart.transform,90,340,440,76);
                FitDialogRect((RectTransform)hud.home.transform,560,340,440,76);
                hud.rewardedContinue.GetComponentInChildren<TextMeshProUGUI>().text="Смотреть видео";
                hud.bonusContinue.GetComponentInChildren<TextMeshProUGUI>().text="Бонусы: 0";
                hud.restart.GetComponentInChildren<TextMeshProUGUI>().text="Заново";
                foreach(var label in dialog.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    label.overflowMode=TextOverflowModes.Ellipsis;
                    label.textWrappingMode=TextWrappingModes.Normal;
                    label.enableAutoSizing=true;
                    var scale=label.GetComponent<AirtistScaledLabel>();
                    if(scale!=null) scale.designFontSize=label==hud.endMessage?25:24;
                    if(label!=hud.endMessage)
                    {
                        var r=label.rectTransform;
                        r.anchorMin=new Vector2(.12f,.18f); r.anchorMax=new Vector2(.88f,.82f);
                        r.offsetMin=r.offsetMax=Vector2.zero;
                    }
                }
                hud.endPanel.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(prefab,Path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            AssetDatabase.SaveAssets();
            return "Close-to-Home button and inset, shortened dialog text saved.";
        }

        private static void FitDialogRect(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=new Vector2(x/1090,1-(y+h)/475);
            r.anchorMax=new Vector2((x+w)/1090,1-y/475);
            r.offsetMin=r.offsetMax=Vector2.zero;
        }

        private static void ApplyStatusStyle()
        {
            var art=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Prototype/Gameplay/GameplayStatusReference.png");
            if(art==null) throw new InvalidOperationException("Approved status artwork missing.");
            var prefab=PrefabUtility.LoadPrefabContents(Path);
            try
            {
                root=prefab.GetComponent<RectTransform>();
                var hud=prefab.GetComponent<AirtistAttemptHud>();
                if(hud==null) throw new InvalidOperationException("Attempt HUD missing.");
                if(hud.separateCaptions) return; // Preserve subsequent Inspector adjustments.
                reference=art;
                var plaque=root.Find("AttemptStatusPanel") as RectTransform;
                if(plaque==null) throw new InvalidOperationException("Attempt plaque missing.");
                Place(plaque,8,117,320,190);
                Divider("StatusDividerVertical",170,132,1.3f,160);
                Divider("StatusDividerHorizontal",24,211,288,1.3f);
                string[] names={"Time","Checks","Energy","Coins"};
                string[] captions={"Время","Проверки","Энергия","Монеты"};
                Rect[] sources={new Rect(25,98,54,65),new Rect(190,97,43,56),new Rect(30,187,47,62),new Rect(186,189,47,58)};
                var values=new[]{hud.timeLabel,hud.clicksLabel,hud.energyLabel,hud.coinsLabel};
                for(int i=0;i<4;i++)
                {
                    bool right=i%2==1; bool bottom=i>=2;
                    float x=right?183:22, y=bottom?226:136;
                    float iw=bottom?42: i==0?46:39;
                    float ih=iw*sources[i].height/sources[i].width;
                    Crop(root,names[i]+"Icon",x,y+10,iw,ih,sources[i]);
                    float tx=right?232:78, tw=right?83:89;
                    Label(root,names[i]+"Caption",captions[i],17,tx,y,tw,25);
                    var value=values[i]; Place(value.rectTransform,tx,y+25,tw,43);
                    value.text=i==0?"—":i==1?"—":i==2?"100/100":"0";
                    value.fontStyle=FontStyles.Bold;
                    value.GetComponent<AirtistScaledLabel>().designFontSize=i==2?24:31;
                    value.overflowMode=TextOverflowModes.Ellipsis;
                    value.transform.SetAsLastSibling();
                }
                var portrait=root.Find("AmeliePortrait") as RectTransform;
                if(portrait!=null)
                {
                    Place(portrait,12,309,310,397);
                    var img=portrait.GetComponent<UnityEngine.UI.RawImage>();
                    img.texture=art; img.uvRect=new Rect(12f/1672,(941f-685)/941,316f/1672,405f/941);
                }
                Box(root,"AmelieSpeechPanel",8,688,320,127,Color.white);
                var nameLabel=root.Find("CharacterName").GetComponent<TextMeshProUGUI>();
                Place(nameLabel.rectTransform,28,697,280,28); nameLabel.transform.SetAsLastSibling();
                nameLabel.GetComponent<AirtistScaledLabel>().designFontSize=23;
                var view=prefab.GetComponent<AirtistGameplayScreen>();
                // Existing references remain valid; use one readable speech area instead of three overlapping paragraphs.
                view.description.gameObject.SetActive(false); view.hint.gameObject.SetActive(false);
                Place(view.feedback.rectTransform,28,732,280,73); view.feedback.transform.SetAsLastSibling();
                view.feedback.text="Присмотрись: что здесь лишнее?";
                view.feedback.GetComponent<AirtistScaledLabel>().designFontSize=21;
                view.feedback.overflowMode=TextOverflowModes.Ellipsis;
                hud.separateCaptions=true;
                hud.endPanel.transform.SetAsLastSibling();
                PrefabUtility.SaveAsPrefabAsset(prefab,Path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }

        private static void Divider(string name,float x,float y,float w,float h)
        {
            var r=Rect(root,name,x,y,w,h);
            var line=r.gameObject.AddComponent<UnityEngine.UI.Image>();
            line.color=new Color(.65f,.46f,.2f,.6f); line.raycastTarget=false;
        }

        private static UnityEngine.UI.Button ChildButton(RectTransform parent,string name,string text,float x,float y,float w,float h)
        {
            var r=Box(parent,name,x,y,w,h,Color.white);
            var button=r.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic=r.GetComponent<UnityEngine.UI.Image>(); button.targetGraphic.raycastTarget=true;
            Label(r,"Label",text,24,8,8,w-16,h-16).alignment=TextAlignmentOptions.Center;
            return button;
        }

        private static void Create()
        {
            root = new GameObject("GameplayScreen", typeof(RectTransform)).GetComponent<RectTransform>();
            root.sizeDelta = new Vector2(1672,941);
            try
            {
                var fit = root.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = 1672f/941;
                var view = root.gameObject.AddComponent<AirtistGameplayScreen>();
                // Shared navigation occupies the first 110 design pixels on every screen.
                Box(root,"CharacterPanel",0,110,334,710,Color.white);
                Crop(root,"AmeliePortrait",65,119,204,366,new Rect(18,90,282,506));
                Label(root,"CharacterName","Амели",25,26,488,280,38);
                view.description = Label(root,"Objective","Найди чужеродные детали.",21,26,531,280,79);
                view.feedback = Label(root,"Feedback","Присмотрись к картине.",20,26,615,280,81);
                view.hint = Label(root,"HintText","Присмотрись: что здесь лишнее?",21,26,702,280,98);
                Box(root,"PaintingFrame",338,110,1334,710,Color.white);
                view.viewport = Rect(root,"ArtworkViewport",348,120,1314,690);
                var backing = view.viewport.gameObject.AddComponent<UnityEngine.UI.Image>();
                backing.color = new Color(.075f,.13f,.13f); backing.raycastTarget = false;
                view.viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
                view.artworkContent = new GameObject("ArtworkContent",typeof(RectTransform)).GetComponent<RectTransform>();
                view.artworkContent.SetParent(view.viewport,false);
                view.artworkContent.anchorMin = view.artworkContent.anchorMax = Vector2.one*.5f;
                view.artworkContent.sizeDelta = new Vector2(900,680);
                var paintingRect = new GameObject("Painting",typeof(RectTransform)).GetComponent<RectTransform>();
                paintingRect.SetParent(view.artworkContent,false); paintingRect.anchorMin=Vector2.zero; paintingRect.anchorMax=Vector2.one;
                paintingRect.offsetMin=paintingRect.offsetMax=Vector2.zero;
                view.painting = paintingRect.gameObject.AddComponent<UnityEngine.UI.Image>();
                view.painting.raycastTarget = true;
                var tap = paintingRect.gameObject.AddComponent<UnityEngine.UI.Button>();
                tap.targetGraphic=view.painting; tap.transition=UnityEngine.UI.Selectable.Transition.None;
                Box(root,"ToolDock",0,820,1672,121,Color.white);
                view.title=Label(root,"PaintingTitle","Название картины",20,25,834,350,57);
                view.progress=Label(root,"FoundProgress","НАЙДЕНО 0 / 3",18,25,899,350,28);
                view.mainTool=Button("MainTool","Инструмент",398,831,254,97,new Color(.3f,.7f,.72f));
                // Individual raw-image illustration crops; no baked labels or hit targets.
                Crop((RectTransform)view.mainTool.transform,"Magnifier",12,14,64,65,new Rect(466,849,68,60));
                var mainLabel = view.mainTool.GetComponentInChildren<TextMeshProUGUI>();
                mainLabel.rectTransform.anchorMin=new Vector2(.32f,.1f); mainLabel.rectTransform.anchorMax=new Vector2(.96f,.9f);
                view.brushes=new UnityEngine.UI.Button[4];
                float[] sourceX={752,905,1060,905};
                for(int i=0;i<4;i++)
                {
                    var button=Button("Brush"+(i+1),"",677+151*i,831,132,97,Color.white);
                    view.brushes[i]=button;
                    UnityEventTools.AddIntPersistentListener(button.onClick,view.SelectBrush,i);
                    var icon=Crop((RectTransform)button.transform,"BrushIcon",14,9,72,76,new Rect(sourceX[i],840,70,70));
                    if(i==3) icon.color=new Color(1,.7f,1);
                    var number=button.GetComponentInChildren<TextMeshProUGUI>(); number.text=(i+1).ToString();
                    number.rectTransform.anchorMin=new Vector2(.65f,.1f); number.rectTransform.anchorMax=new Vector2(.94f,.7f);
                }
                view.help=Button("Help","?",1450,839,86,82,new Color(.3f,.7f,.72f));
                PrefabUtility.SaveAsPrefabAsset(root.gameObject,Path);
            }
            finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
        }

        private static RectTransform Rect(RectTransform parent,string name,float x,float y,float w,float h)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false);
            float pw=parent==root?1672:parent.rect.width, ph=parent==root?941:parent.rect.height;
            r.anchorMin=new Vector2(x/pw,1-(y+h)/ph); r.anchorMax=new Vector2((x+w)/pw,1-y/ph);
            r.offsetMin=r.offsetMax=Vector2.zero; return r;
        }
        private static RectTransform Box(RectTransform parent,string name,float x,float y,float w,float h,Color tint)
        {
            var r=Rect(parent,name,x,y,w,h); var img=r.gameObject.AddComponent<UnityEngine.UI.Image>();
            img.sprite=panel; img.type=UnityEngine.UI.Image.Type.Sliced; img.color=tint; img.raycastTarget=false; return r;
        }
        private static UnityEngine.UI.RawImage Crop(RectTransform parent,string name,float x,float y,float w,float h,Rect source)
        {
            var r=Rect(parent,name,x,y,w,h); var img=r.gameObject.AddComponent<UnityEngine.UI.RawImage>();
            img.texture=reference; img.uvRect=new Rect(source.x/1672,(941-source.yMax)/941,source.width/1672,source.height/941);
            img.raycastTarget=false; return img;
        }
        private static TextMeshProUGUI Label(RectTransform parent,string name,string text,float size,float x,float y,float w,float h)
        {
            var r=Rect(parent,name,x,y,w,h); var label=r.gameObject.AddComponent<TextMeshProUGUI>();
            label.font=font; label.text=text; label.fontSize=size; label.color=Ink; label.raycastTarget=false;
            label.alignment=TextAlignmentOptions.MidlineLeft;
            var scale=r.gameObject.AddComponent<AirtistScaledLabel>(); scale.artboard=root; scale.designFontSize=size;
            return label;
        }
        private static UnityEngine.UI.Button Button(string name,string text,float x,float y,float w,float h,Color color)
        {
            var r=Box(root,name,x,y,w,h,color); var button=r.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic=r.GetComponent<UnityEngine.UI.Image>(); button.targetGraphic.raycastTarget=true;
            var label=Label(r,"Label",text,25,4,4,w-8,h-8); label.alignment=TextAlignmentOptions.Center;
            return button;
        }
    }
}
