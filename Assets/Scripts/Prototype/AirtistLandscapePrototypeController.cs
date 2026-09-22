using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Airtist.Prototype
{
    /// <summary>
    /// Runtime UI for the first landscape prototype. It deliberately keeps all content local so the
    /// flow can be played before live content, payments, adverts and cloud progress are connected.
    /// </summary>
    public sealed partial class AirtistLandscapePrototypeController : MonoBehaviour
    {
        private enum Page
        {
            Home,
            WorldMap,
            Museum,
            Gallery,
            Found,
            DailyBonus,
            Collection,
            Store,
            Profile,
            MuseumPreview
        }

        private static readonly Color Ink = Hex("20303F");
        private static readonly Color Cream = Hex("FFF4DF");
        private static readonly Color Teal = Hex("367C7D");
        private static readonly Color DeepTeal = Hex("1E5055");
        private static readonly Color Gold = Hex("F2B84A");
        private static readonly Color Coral = Hex("F87964");
        private static readonly Color Sand = Hex("E8D6BD");
        private static readonly Color Moss = Hex("7BAA73");
        private static readonly Color Parchment = Hex("FFF8E9");
        private static readonly Color AntiqueGold = Hex("C99842");
        private static readonly Color Rust = Hex("A64B31");
        private static readonly Color MorningBlue = Hex("D8E8E7");
        private static readonly ChapterData[] Chapters = WithLouvreExpansion(new[]
        {
            new ChapterData(
                "Мона Лиза",
                "Найди 3 AI-дорисовки",
                "Усик, часы и игрушка больше не отвлекают от портрета. Работа восстановлена.",
                "Лувр показывает «Мону Лизу» Леонардо да Винчи в зале 711. Её мягкие переходы светотени называют сфумато.",
                "Начни с лица: у сфумато нет жёстких, чужеродных контуров.",
                new[]
                {
                    new ArtifactData("накладной усик", "Присмотрись к верхней губе Моны Лизы.", new Vector2(.46f,.697f), new Vector2(80,64), ArtifactVisual.Moustache),
                    new ArtifactData("наручные часы", "Современные часы оказались рядом с запястьем.", new Vector2(.335f,.24f), new Vector2(80,80), ArtifactVisual.TimeTag),
                    new ArtifactData("резиновая уточка", "Проверь воду в пейзаже слева от фигуры.", new Vector2(.165f,.59f), new Vector2(80,70), ArtifactVisual.Badge)
                }),
            new ChapterData(
                "Свобода, ведущая народ",
                "Найди 5 AI-дорисовок",
                "Пять добавленных предметов убраны. Историческая сцена снова свободна от чужих деталей.",
                "Делакруа написал картину после Июльской революции 1830 года. В Лувре она находится в зале 700, Salle Mollien.",
                "Ищи современные символы там, где художник использовал только исторические детали.",
                new[]
                {
                    new ArtifactData("наушники", "У юноши справа появились современные наушники.", new Vector2(.75f,.63f), new Vector2(80,90), ArtifactVisual.Badge),
                    new ArtifactData("стаканчик кофе", "Осмотри левую часть баррикады.", new Vector2(.11f,.265f), new Vector2(80,90), ArtifactVisual.TimeTag),
                    new ArtifactData("бумажный самолётик", "Посмотри на дымное небо справа от флага.", new Vector2(.82f,.85f), new Vector2(90,70), ArtifactVisual.Signal),
                    new ArtifactData("кроссовок", "Проверь обувь возле центральной фигуры.", new Vector2(.59f,.33f), new Vector2(90,70), ArtifactVisual.Badge),
                    new ArtifactData("потемневшая монета", "Увеличь обломки слева внизу: на тёмной балке лежит монета.", new Vector2(.225f,.242f), new Vector2(72,64), ArtifactVisual.Badge)
                }),
            new ChapterData(
                "Плот «Медузы»",
                "Найди 6 AI-дорисовок",
                "Шесть добавленных предметов исчезли. Перед тобой снова исходная композиция Жерико.",
                "Жерико показал эту картину на Салоне 1819 года: она рассказывает о крушении фрегата «Медуза», после которого выжили пятнадцать человек.",
                "Начни с горизонта: жесты фигур ведут к дальнему силуэту.",
                new[]
                {
                    new ArtifactData("спасательный круг", "Современный круг лежит у левого края плота.", new Vector2(.11f,.23f), new Vector2(90,90), ArtifactVisual.Signal),
                    new ArtifactData("пластиковая бутылка", "Осмотри доски внизу, чуть правее центра.", new Vector2(.57f,.10f), new Vector2(80,95), ArtifactVisual.Badge),
                    new ArtifactData("пляжный мяч", "Проверь правый край плота.", new Vector2(.90f,.26f), new Vector2(85,85), ArtifactVisual.TimeTag),
                    new ArtifactData("солнечные очки", "У фигуры возле центра появились современные очки.", new Vector2(.456f,.413f), new Vector2(85,64), ArtifactVisual.Badge),
                    new ArtifactData("дорожный конус", "Проверь нижнюю левую часть плота.", new Vector2(.305f,.225f), new Vector2(80,90), ArtifactVisual.Signal),
                    new ArtifactData("старый железный ключ", "Увеличь переднюю балку плота: слева от её обвязки лежит ключ.", new Vector2(.37f,.105f), new Vector2(72,64), ArtifactVisual.TimeTag)
                })
        });

        [SerializeField] private Sprite homeBackground;
        [SerializeField] private Sprite galleryBackground;
        [SerializeField] private Sprite portrait;
        [SerializeField] private Sprite[] louvrePaintings;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private AirtistHomeScreen homeScreenPrefab;
        [SerializeField] private AirtistWorldMapScreen worldMapPrefab;
        [SerializeField] private AirtistMyGalleryScreen myGalleryPrefab;
        [SerializeField] private AirtistStoreScreen storeScreenPrefab;
        public void ConfigureStore(AirtistStoreScreen prefab) => storeScreenPrefab = prefab;
        [SerializeField] private Sprite galleryMenuPanel;
        public void ConfigureGalleryMenuPanel(Sprite sprite) => galleryMenuPanel = sprite;
        private AirtistMyGalleryScreen myGallery;
        private AirtistMyGalleryScreen museumGallery;
        public void ConfigureMyGallery(AirtistMyGalleryScreen prefab) => myGalleryPrefab = prefab;
        private AirtistHomeScreen illustratedHome;
        private static readonly string[] ChapterIds = WithLouvreIds(new[] { "mona-lisa", "liberty", "medusa" });
        private string dailyClaimUtc = "";

        private readonly Dictionary<Page, GameObject> pages = new();
        private readonly List<TextMeshProUGUI> discoveryBonusLabels = new();
        private readonly List<TextMeshProUGUI> hintBonusLabels = new();
        private readonly List<ChapterCardView> chapterCards = new();
        private readonly List<CollectionCardView> collectionCards = new();
        private readonly List<ArtifactTargetView> galleryArtifactTargets = new();
        private Sprite roundedSprite;
        private Sprite whiteSprite;
        private readonly bool[] chapterCollected = new bool[Chapters.Length];
        private readonly bool[] hintUsedForChapter = new bool[Chapters.Length];
        private readonly bool[][] artifactFound = CreateArtifactState();
        private int selectedChapter;
        private int hintCount = 3;
        private bool dailyBonusClaimed;
        private bool rewardedHintClaimed;
        private bool initialized;
        private TextMeshProUGUI collectionProgress;
        private TextMeshProUGUI profileFoundCount;
        private TextMeshProUGUI profileCollectionCount;
        private UnityEngine.UI.Button collectionNextButton;
        private TextMeshProUGUI collectionNextButtonLabel;
        private TextMeshProUGUI mapProgress;
        private TextMeshProUGUI galleryTitle;
        private TextMeshProUGUI galleryTargetProgress;
        private TextMeshProUGUI galleryDescription;
        private TextMeshProUGUI galleryFeedback;
        private TextMeshProUGUI galleryHint;
        private UnityEngine.UI.Image galleryPainting;
        private PanZoomArtwork galleryPanZoom;
        private UnityEngine.UI.Button galleryHintButton;
        private TextMeshProUGUI galleryHintButtonLabel;
        private UnityEngine.UI.Button dailyHomeButton;
        private TextMeshProUGUI dailyHomeButtonLabel;
        private TextMeshProUGUI dailyHomeState;
        private UnityEngine.UI.Button dailyClaimButton;
        private TextMeshProUGUI dailyClaimButtonLabel;
        private UnityEngine.UI.Button rewardedHintButton;
        private TextMeshProUGUI rewardedHintButtonLabel;
        private TextMeshProUGUI rewardedHintState;

        public void Configure(Sprite homeBackdrop, Sprite backdrop, Sprite featuredPortrait, Sprite[] featuredPaintings, TMP_FontAsset uiFont)
        {
            homeBackground = homeBackdrop;
            galleryBackground = backdrop;
            portrait = featuredPortrait;
            louvrePaintings = featuredPaintings;
            font = uiFont;
        }

        public void ConfigureHomeScreen(AirtistHomeScreen prefab)
        {
            homeScreenPrefab = prefab;
        }

        public void ConfigureWorldMap(AirtistWorldMapScreen prefab) => worldMapPrefab = prefab;

        private void LoadProgress()
        {
            var state = AirtistProgress.Load();
            selectedChapter = Mathf.Clamp(state.selectedChapter, 0, Chapters.Length - 1);
            hintCount = Mathf.Clamp(state.hints, 0, 9999);
            dailyClaimUtc = state.dailyUtc ?? "";
            dailyBonusClaimed = dailyClaimUtc == DateTime.UtcNow.ToString("yyyy-MM-dd");
            rewardedHintClaimed = state.rewardedHintClaimed;
            Array.Clear(replaying,0,replaying.Length);
            Array.Clear(restorations,0,restorations.Length);
            Array.Clear(areaHintTargets,0,areaHintTargets.Length);
            Array.Clear(exactHintTargets,0,exactHintTargets.Length);
            for (int i = 0; i < Chapters.Length; i++)
            {
                var entry = Array.Find(state.chapters, c => c != null && c.id == ChapterIds[i]);
                if (entry == null) continue;
                int oldCount=state.contentVersion<2 ? 3 : i==0 ? 3 : i==1 ? 4 : 5;
                int oldMask=(1<<oldCount)-1;
                bool legacyCompleted=i<3 && state.contentVersion<3 && (entry.collected || (entry.foundMask & oldMask)==oldMask);
                for (int j = 0; j < artifactFound[i].Length; j++) artifactFound[i][j] = legacyCompleted || (entry.foundMask & (1 << j)) != 0;
                replaying[i]=entry.replaying && entry.collected;
                chapterCollected[i] = entry.collected && (IsChapterComplete(i) || replaying[i]);
                restorations[i]=entry.restoration ?? new AirtistRestorationRecord();
                if(chapterCollected[i] && restorations[i].bestMask==0) restorations[i].bestMask=1;
                hintUsedForChapter[i] = entry.hintUsed;
                areaHintTargets[i]=Mathf.Clamp(entry.areaHintTarget,0,artifactFound[i].Length);
                exactHintTargets[i]=Mathf.Clamp(entry.exactHintTarget,0,artifactFound[i].Length);
            }
            LoadAttemptProgress(state);
        }

        private void SaveProgress()
        {
            if (!initialized) return;
            var state = new AirtistProgress { contentVersion = 3, selectedChapter = selectedChapter, hints = hintCount,
                dailyUtc = dailyClaimUtc, rewardedHintClaimed = rewardedHintClaimed,
                chapters = new AirtistProgress.ChapterProgress[Chapters.Length] };
            for (int i = 0; i < Chapters.Length; i++)
            {
                int mask = 0;
                for (int j = 0; j < artifactFound[i].Length; j++) if (artifactFound[i][j]) mask |= 1 << j;
                state.chapters[i] = new AirtistProgress.ChapterProgress { id = ChapterIds[i], foundMask = mask,
                    collected = chapterCollected[i], hintUsed = hintUsedForChapter[i],
                    replaying=replaying[i], restoration=restorations[i],
                    areaHintTarget=areaHintTargets[i],exactHintTarget=exactHintTargets[i] };
            }
            SaveAttemptProgress(state);
            state.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if(!initialized) return;
            TickAttemptClock(); appPaused=paused; clockStamp=Time.realtimeSinceStartupAsDouble;
            if(paused) SaveProgress();
        }
        private void OnApplicationQuit() => SaveProgress();

        private void ContinueAdventure()
        {
            int next = (!chapterCollected[selectedChapter] || replaying[selectedChapter]) && IsChapterUnlocked(selectedChapter)
                ? selectedChapter : GetNextUncollectedChapter();
            if (next < 0) { Show(Page.Collection); return; }
            OpenChapter(next);
        }

        private void Awake()
        {
            if (Application.isPlaying)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            Application.targetFrameRate = 60;
            LoadProgress();
            BuildInterface();
            Show(Page.Home);
        }

        private void BuildInterface()
        {
            whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
            roundedSprite = CreateRoundedSprite();

            BuildHome();
            BuildWorldMap();
            BuildMuseumPreview();
            BuildMuseum();
            BuildGallery();
            BuildFound();
            BuildDailyBonus();
            BuildCollection();
            BuildStore();
            if (myGalleryPrefab == null) BuildProfile();
            BuildUniversalHeader();
            ApplyFullscreenPresentation();
            BuildDeveloperReset();
            ApplyApprovedPresentation();
            BuildEnergyShop();
            UpdateProgressLabels();
        }

        private void BuildHome()
        {
            RectTransform page = CreatePage(Page.Home, "Home");
            if (homeScreenPrefab != null)
            {
                page.GetComponent<UnityEngine.UI.Image>().color = DeepTeal;
                illustratedHome = Instantiate(homeScreenPrefab, page, false);
                illustratedHome.name = "HomeIllustrated";
                illustratedHome.Bind(new Action[]
                {
                    OpenDailyBonus,
                    () => Show(Page.WorldMap),
                    () => Show(Page.Collection),
                    () => Show(Page.Store),
                    () => Show(Page.Profile),
                    ContinueAdventure,
                    OpenDailyBonus,
                    () => Show(Page.WorldMap)
                });
                return;
            }
            CreateImage(page, "HomeBackdrop", homeBackground != null ? homeBackground : galleryBackground, Color.white, Anchor.Stretch, Vector2.zero, Vector2.zero, false);
            CreatePanel(page, "HomeWarmthVeil", new Color(1f, 0.95f, 0.83f, 0.10f), Anchor.Stretch, Vector2.zero, Vector2.zero);
            BuildHomeHeader(page);

            RectTransform headlinePlate = CreatePanel(page, "HomeTitlePlate", new Color(Parchment.r, Parchment.g, Parchment.b, 0.91f), Anchor.TopLeft, new Vector2(82, -160), new Vector2(800, 238));
            CreatePanel(headlinePlate, "HeadlineGoldRule", AntiqueGold, Anchor.Left, new Vector2(20, 0), new Vector2(7, 180));
            CreateLabel(headlinePlate, "Старинные картины.", 49, Ink, Anchor.TopLeft, new Vector2(48, -39), new Vector2(700, 58), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(headlinePlate, "Современные следы.", 50, Rust, Anchor.TopLeft, new Vector2(48, -94), new Vector2(700, 58), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(headlinePlate, "Искусство хранит тайны.\nА ты умеешь их находить?", 20, Ink * new Color(1f, 1f, 1f, 0.8f), Anchor.TopLeft, new Vector2(50, -161), new Vector2(610, 54), TextAlignmentOptions.Left);

            RectTransform routeFrame = CreatePanel(page, "TodayRouteFrame", AntiqueGold, Anchor.BottomLeft, new Vector2(78, 60), new Vector2(960, 306));
            RectTransform routeCard = CreatePanel(routeFrame, "TodayRoute", new Color(Parchment.r, Parchment.g, Parchment.b, 0.97f), Anchor.Center, Vector2.zero, new Vector2(938, 284));
            CreateLabel(routeCard, "◈  СЕГОДНЯШНИЙ МАРШРУТ", 16, DeepTeal, Anchor.TopLeft, new Vector2(34, -26), new Vector2(420, 26), TextAlignmentOptions.Left, FontStyles.Bold);
            RectTransform routeThumbnailFrame = CreatePanel(routeCard, "RouteThumbnailFrame", AntiqueGold, Anchor.TopLeft, new Vector2(32, -66), new Vector2(246, 118));
            CreateImage(routeThumbnailFrame, "RouteThumbnail", homeBackground, Color.white, Anchor.Center, Vector2.zero, new Vector2(230, 102), true);
            CreateLabel(routeCard, "Музей у Сены", 35, Ink, Anchor.TopLeft, new Vector2(306, -65), new Vector2(440, 44), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(routeCard, RouteSummary, 18, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.TopLeft, new Vector2(308, -112), new Vector2(490, 28), TextAlignmentOptions.Left);
            CreateFramedButton(routeCard, "Продолжить поиск", Rust, Cream, Anchor.BottomLeft, new Vector2(32, 28), new Vector2(368, 68), ContinueAdventure, 19);
            RectTransform dailyBonusPill = CreatePanel(routeCard, "DailyBonusPill", new Color(Gold.r, Gold.g, Gold.b, 0.9f), Anchor.BottomRight, new Vector2(-30, 94), new Vector2(326, 40));
            dailyHomeState = CreateLabel(dailyBonusPill, "Ежедневный бонус: +1 подсказка", 14, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(302, 24), TextAlignmentOptions.Center, FontStyles.Bold);
            dailyHomeButton = CreateFramedButton(routeCard, "Забрать", Gold, DeepTeal, Anchor.BottomRight, new Vector2(-30, 26), new Vector2(230, 62), OpenDailyBonus, 18);
            dailyHomeButtonLabel = GetButtonLabel(dailyHomeButton);

            RectTransform artCard = CreatePanel(page, "AmelieFrame", AntiqueGold, Anchor.BottomRight, new Vector2(-80, 126), new Vector2(520, 720));
            RectTransform artInner = CreatePanel(artCard, "AmeliePortraitMat", new Color(Parchment.r, Parchment.g, Parchment.b, 0.30f), Anchor.Center, new Vector2(0, 0), new Vector2(496, 696));
            CreateImage(artInner, "AmeliePortrait", portrait, Color.white, Anchor.Bottom, new Vector2(0, 16), new Vector2(474, 664), true);
            RectTransform namePlaque = CreatePanel(artCard, "AmelieNamePlaque", Parchment, Anchor.Bottom, new Vector2(0, 22), new Vector2(334, 78));
            CreateLabel(namePlaque, "АМЕЛИ", 23, Ink, Anchor.Top, new Vector2(0, -16), new Vector2(260, 30), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(namePlaque, "СТУДЕНТКА ЖИВОПИСИ", 13, Rust, Anchor.Bottom, new Vector2(0, 13), new Vector2(270, 22), TextAlignmentOptions.Center, FontStyles.Bold);

            CreateFramedButton(page, "Открыть карту мира", Teal, Cream, Anchor.BottomRight, new Vector2(-598, 70), new Vector2(400, 76), () => Show(Page.WorldMap), 20);
        }

        private void BuildHomeHeader(RectTransform page)
        {
            RectTransform headerFrame = CreatePanel(page, "HomeHeaderFrame", AntiqueGold, Anchor.Top, new Vector2(0, -54), new Vector2(1760, 100));
            RectTransform header = CreatePanel(headerFrame, "HomeHeader", new Color(Parchment.r, Parchment.g, Parchment.b, 0.96f), Anchor.Center, Vector2.zero, new Vector2(1740, 80));
            CreateLabel(header, "AIrtist", 48, Ink, Anchor.Left, new Vector2(52, 0), new Vector2(220, 54), TextAlignmentOptions.Left, FontStyles.Bold | FontStyles.Italic);
            CreateLabel(header, "Париж · первая выставка", 18, Ink * new Color(1f, 1f, 1f, 0.73f), Anchor.Left, new Vector2(292, 0), new Vector2(360, 30), TextAlignmentOptions.Left);

            RectTransform bonus = CreatePanel(header, "HomeBonus", Gold, Anchor.Right, new Vector2(-622, 0), new Vector2(142, 58));
            CreateLabel(bonus, "БОНУСЫ  +3", 14, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(124, 28), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateFramedButton(header, "Карта", Parchment, DeepTeal, Anchor.Right, new Vector2(-464, 0), new Vector2(112, 58), () => Show(Page.WorldMap), 16);
            CreateFramedButton(header, "Коллекция", Parchment, DeepTeal, Anchor.Right, new Vector2(-325, 0), new Vector2(140, 58), () => Show(Page.Collection), 15);
            CreateFramedButton(header, "Магазин", Parchment, DeepTeal, Anchor.Right, new Vector2(-183, 0), new Vector2(120, 58), () => Show(Page.Store), 15);
            CreateFramedButton(header, "Профиль", Parchment, DeepTeal, Anchor.Right, new Vector2(-54, 0), new Vector2(108, 58), () => Show(Page.Profile), 15);
        }

        private void BuildWorldMap()
        {
            if (worldMapPrefab != null)
            {
                RectTransform mapPage = CreatePage(Page.WorldMap, "WorldMap");
                mapPage.GetComponent<UnityEngine.UI.Image>().color = DeepTeal;
                var worldMap = Instantiate(worldMapPrefab, mapPage, false);
                worldMap.Bind(() => Show(Page.Home), () => Show(Page.Collection), () => Show(Page.Profile));
                BuildMuseumPins(worldMap.MapContent);
                worldMap.UseUniversalNavigation();
                StyleMapZoomButtons(worldMap);
                return;
            }
            RectTransform page = CreatePage(Page.WorldMap, "WorldMap");
            BuildHeader(page, "Мир музеев");
            CreateLabel(page, "Выбери музей", 47, Ink, Anchor.TopLeft, new Vector2(84, -164), new Vector2(530, 62), TextAlignmentOptions.Left, FontStyles.Bold);
            mapProgress = CreateLabel(page, "Открыто: 1 из 10 музеев", 23, Ink * new Color(1f, 1f, 1f, 0.7f), Anchor.TopLeft, new Vector2(86, -226), new Vector2(500, 34), TextAlignmentOptions.Left);

            RectTransform map = CreatePanel(page, "IllustratedMap", new Color(0.73f, 0.87f, 0.85f, 1f), Anchor.Bottom, new Vector2(0, 70), new Vector2(1740, 650));
            CreateLabel(map, "Европа", 84, new Color(1f, 1f, 1f, 0.48f), Anchor.Center, new Vector2(0, 34), new Vector2(540, 100), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(map, "Линии маршрутов · коллекция · факты", 23, DeepTeal * new Color(1f, 1f, 1f, 0.75f), Anchor.Bottom, new Vector2(0, 42), new Vector2(620, 34), TextAlignmentOptions.Center);

            CreateMuseumNode(map, "Музей у Сены", "Париж", new Vector2(-350, 68), true, () => Show(Page.Museum));
            CreateMuseumNode(map, "Национальная галерея", "Лондон", new Vector2(-70, 168), false, null);
            CreateMuseumNode(map, "Прадо", "Мадрид", new Vector2(190, -36), false, null);
            CreateMuseumNode(map, "Уффици", "Флоренция", new Vector2(420, 66), false, null);

            RectTransform note = CreatePanel(page, "MapNote", Cream, Anchor.BottomRight, new Vector2(-88, 92), new Vector2(405, 112));
            CreateLabel(note, "Каждый музей — новая глава\nи новая коллекция картин.", 20, Ink, Anchor.Center, Vector2.zero, new Vector2(350, 72), TextAlignmentOptions.Center, FontStyles.Bold);
        }

        private void BuildMuseum()
        {
            RectTransform page = CreatePage(Page.Museum, "Museum");
            // Reuse the editable three-column, vertically scrolling catalog for all ten works.
            if (myGalleryPrefab != null)
            {
                museumGallery = Instantiate(myGalleryPrefab, page, false);
                museumGallery.name = "LouvrePaintingSelection";
                museumGallery.Bind(
                    index => index >= 0 && index < Chapters.Length && chapterCollected[index],
                    index => index >= 0 && index < Chapters.Length && IsChapterUnlocked(index),
                    index => index >= 0 && index < Chapters.Length ? FoundArtifactCount(index) : 0,
                    index => { if (index >= 0 && index < Chapters.Length) OpenChapter(index); },
                    OpenMuseumPreview);
                if (museumGallery.nextPainting != null)
                    museumGallery.nextPainting.onClick.AddListener(OpenNextUncollectedChapterFromCollection);
                return;
            }
            AddJourneyBackdrop(page);
            BuildHeader(page, "Лувр · Париж");
            CreateLabel(page, "Глава 1. След в галерее", 47, Ink, Anchor.TopLeft, new Vector2(84, -164), new Vector2(720, 62), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(page, "Собирай карточки картин и открывай короткие истории о художниках.", 23, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.TopLeft, new Vector2(86, -226), new Vector2(900, 34), TextAlignmentOptions.Left);

            CreateChapterCard(page, 0, new Vector2(-560, -82));
            CreateChapterCard(page, 1, new Vector2(0, -82));
            CreateChapterCard(page, 2, new Vector2(560, -82));

            RectTransform fact = CreateJourneyPanel(page, "MuseumFact", Anchor.Bottom, new Vector2(0, 82), new Vector2(1450, 118));
            CreateLabel(fact, "ФАКТ", 17, Coral, Anchor.Left, new Vector2(42, 0), new Vector2(90, 34), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(fact, "Музеи — это не только коллекции: архитектура и способ показа искусства тоже становятся частью истории.", 22, Ink, Anchor.Left, new Vector2(154, 0), new Vector2(1180, 62), TextAlignmentOptions.Left);
            ApplyJourneyTypography(page);
        }

        private void BuildGallery()
        {
            if (gameplayPrefab != null) { BuildEditableGameplay(); return; }
            RectTransform page = CreatePage(Page.Gallery, "Gallery");
            CreateImage(page, "GalleryBackdrop", galleryBackground, new Color(0.24f, 0.36f, 0.39f, 1f), Anchor.Stretch, Vector2.zero, Vector2.zero, false);
            CreatePanel(page, "GalleryVeil", new Color(0.08f, 0.18f, 0.21f, 0.38f), Anchor.Stretch, Vector2.zero, Vector2.zero);
            BuildHeader(page, $"Лувр · {Chapters.Length} картин");
            galleryTitle = CreateLabel(page, "Найди AI-дорисовку", 36, Cream, Anchor.TopLeft, new Vector2(84, -180), new Vector2(540, 130), TextAlignmentOptions.Left, FontStyles.Bold);
            galleryTargetProgress = CreateLabel(page, "НАЙДЕНО 0 / 3", 20, Gold, Anchor.TopRight, new Vector2(-84, -160), new Vector2(250, 40), TextAlignmentOptions.Right, FontStyles.Bold);
            galleryDescription = CreateLabel(page, "Смотри на картину внимательно: инородная деталь часто прячется на самом видном месте.", 27, Cream * new Color(1f, 1f, 1f, 0.83f), Anchor.TopLeft, new Vector2(86, -330), new Vector2(530, 125), TextAlignmentOptions.Left);
            galleryFeedback = CreateLabel(page, "Тапни по детали, которая выглядит чужой для этой картины.", 26, Gold, Anchor.TopLeft, new Vector2(86, -490), new Vector2(530, 180), TextAlignmentOptions.Left, FontStyles.Bold);

            RectTransform frame = CreatePanel(page, "GoldFrame", Gold, Anchor.Center, new Vector2(240, -30), new Vector2(1020, 690));
            RectTransform viewport = CreatePanel(frame, "ArtworkViewport", DeepTeal, Anchor.Center, Vector2.zero, new Vector2(970, 640));
            viewport.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();

            GameObject artworkContentObject = new GameObject("ArtworkContent", typeof(RectTransform));
            artworkContentObject.transform.SetParent(viewport, false);
            RectTransform artworkContent = artworkContentObject.GetComponent<RectTransform>();
            SetAnchor(artworkContent, Anchor.Center, Vector2.zero, new Vector2(530, 600));

            RectTransform painting = CreateImage(artworkContent, "Painting", GetChapterArtwork(selectedChapter), Color.white, Anchor.Stretch, Vector2.zero, Vector2.zero, false);
            galleryPainting = painting.GetComponent<UnityEngine.UI.Image>();
            galleryPainting.raycastTarget = true;
            UnityEngine.UI.Button tapSurface = painting.gameObject.AddComponent<UnityEngine.UI.Button>();
            tapSurface.targetGraphic = galleryPainting;
            tapSurface.transition = UnityEngine.UI.Selectable.Transition.None;
            tapSurface.onClick.AddListener(RegisterIncorrectTap);

            galleryPanZoom = viewport.gameObject.AddComponent<PanZoomArtwork>();
            galleryPanZoom.Configure(viewport, artworkContent);
            galleryPanZoom.SetArtworkSize(CalculateArtworkDisplaySize(GetChapterArtwork(selectedChapter)));

            for (int chapterIndex = 0; chapterIndex < Chapters.Length; chapterIndex++)
            {
                ArtifactData[] artifacts = Chapters[chapterIndex].Artifacts;
                for (int artifactIndex = 0; artifactIndex < artifacts.Length; artifactIndex++)
                {
                    galleryArtifactTargets.Add(CreateArtifactTarget(artworkContent, chapterIndex, artifactIndex, artifacts[artifactIndex]));
                }
            }

            galleryHint = CreateLabel(page, "Увеличивай картину кнопками + / − и перетаскивай её пальцем или мышью.", 20, Cream, Anchor.Bottom, new Vector2(-160, 56), new Vector2(780, 32), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateButton(page, "−", Sand, DeepTeal, Anchor.BottomRight, new Vector2(-440, 46), new Vector2(76, 68), galleryPanZoom.ZoomOut, 30);
            CreateButton(page, "+", Sand, DeepTeal, Anchor.BottomRight, new Vector2(-346, 46), new Vector2(76, 68), galleryPanZoom.ZoomIn, 30);
            galleryHintButton = CreateButton(page, "Подсказка", Gold, DeepTeal, Anchor.BottomRight, new Vector2(-80, 46), new Vector2(230, 58), UseHintForCurrentChapter, 18);
            galleryHintButtonLabel = GetButtonLabel(galleryHintButton);
        }

        private void BuildFound()
        {
            BuildRestorationScreen();
        }

        private void BuildDailyBonus()
        {
            RectTransform page = CreatePage(Page.DailyBonus, "DailyBonus");
            AirtistFlexibleArtboard.Fill(page);
            page.GetComponent<UnityEngine.UI.Image>().color=new Color(.91f,.91f,.83f);
            BuildHeader(page, "Ежедневный маршрут");
            var card=CreatePanel(page,"DailyBonusCard",new Color(.99f,.96f,.88f),Anchor.Stretch,Vector2.zero,Vector2.zero);
            ResultRect(card,.245f,.11f,.755f,.82f);
            ResultText(page,"DailyHeading","Твой ежедневный подарок",38,.28f,.69f,.72f,.78f);
            ResultText(page,"DailySubtitle","Небольшая помощь для новых открытий",23,.28f,.59f,.72f,.68f);
            var reward=CreatePanel(page,"DailyReward",new Color(.96f,.79f,.49f),Anchor.Stretch,Vector2.zero,Vector2.zero);
            ResultRect(reward,.425f,.365f,.575f,.565f);
            ResultText(page,"DailyAmount","+1",56,.44f,.425f,.56f,.535f);
            ResultText(page,"DailyRewardCaption","подсказка",23,.43f,.375f,.57f,.425f);
            ResultText(page,"DailySchedule","Возвращайся каждый день.\nНовый подарок — в 00:00 UTC.",21,.29f,.26f,.71f,.345f);
            dailyClaimButton = CreateButton(page, "Забрать бонус", new Color(.79f,.35f,.25f), Cream, Anchor.Center, Vector2.zero, new Vector2(360,80), ClaimDailyBonus);
            ResultRect((RectTransform)dailyClaimButton.transform,.34f,.155f,.66f,.245f);
            dailyClaimButton.image.raycastPadding=Vector4.zero;
            dailyClaimButtonLabel = GetButtonLabel(dailyClaimButton);
            Stretch(dailyClaimButtonLabel.rectTransform);
            dailyClaimButtonLabel.rectTransform.offsetMin=new Vector2(14,5);
            dailyClaimButtonLabel.rectTransform.offsetMax=new Vector2(-14,-5);
            var scale=dailyClaimButtonLabel.gameObject.AddComponent<AirtistScaledLabel>();
            scale.artboard=page; scale.designFontSize=24;
        }

        private void BuildCollection()
        {
            if (myGalleryPrefab != null)
            {
                var galleryPage = CreatePage(Page.Collection, "MyGallery");
                myGallery = Instantiate(myGalleryPrefab, galleryPage, false);
                collectionNextButton=myGallery.nextPainting;
                collectionNextButtonLabel=GetButtonLabel(collectionNextButton);
                if(collectionNextButton!=null) collectionNextButton.onClick.AddListener(OpenNextUncollectedChapterFromCollection);
                myGallery.Bind(
                    index => index >= 0 && index < Chapters.Length && chapterCollected[index],
                    index => index >= 0 && index < Chapters.Length && IsChapterUnlocked(index),
                    index => index >= 0 && index < Chapters.Length ? FoundArtifactCount(index) : 0,
                    index => { if (index >= 0 && index < Chapters.Length) OpenChapter(index); },
                    OpenMuseumPreview);
                return;
            }
            RectTransform page = CreatePage(Page.Collection, "Collection");
            BuildHeader(page, "Моя коллекция");
            CreateLabel(page, "Картины с возвращённой историей", 45, Ink, Anchor.TopLeft, new Vector2(84, -164), new Vector2(860, 60), TextAlignmentOptions.Left, FontStyles.Bold);
            collectionProgress = CreateLabel(page, "0 из 3 картин в учебной главе", 23, Ink * new Color(1f, 1f, 1f, 0.7f), Anchor.TopLeft, new Vector2(86, -226), new Vector2(620, 34), TextAlignmentOptions.Left);

            CreateCollectionCard(page, 0, new Vector2(-520, -42));
            CreateCollectionCard(page, 1, new Vector2(0, -42));
            CreateCollectionCard(page, 2, new Vector2(520, -42));
            CreateLabel(page, "Учебная глава хранит три картины. В полной игре появятся серии, факты и тематические альбомы.", 22, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.Bottom, new Vector2(0, 90), new Vector2(1300, 34), TextAlignmentOptions.Center);
            collectionNextButton = CreateButton(page, "К следующей картине", Coral, Cream, Anchor.Bottom, new Vector2(0, 26), new Vector2(380, 64), OpenNextUncollectedChapterFromCollection, 19);
            collectionNextButtonLabel = GetButtonLabel(collectionNextButton);
        }

        private void BuildStore()
        {
            if (storeScreenPrefab != null)
            {
                var storePage = CreatePage(Page.Store, "Store");
                Instantiate(storeScreenPrefab, storePage, false);
                return;
            }
            RectTransform page = CreatePage(Page.Store, "Store");
            BuildHeader(page, "Магазин");
            CreateLabel(page, "Комфортный поиск, без давления", 46, Ink, Anchor.Top, new Vector2(0, -150), new Vector2(900, 62), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(page, "Это макет монетизации: реальные покупки и реклама в прототип не подключены.", 22, Ink * new Color(1f, 1f, 1f, 0.7f), Anchor.Top, new Vector2(0, -215), new Vector2(1000, 34), TextAlignmentOptions.Center);

            rewardedHintButton = CreateStoreCard(page, "Подсказка", "Мягкая помощь в сложной картине", Gold, new Vector2(-520, -60), "Смотреть рекламу", ClaimRewardedHint);
            rewardedHintButtonLabel = GetButtonLabel(rewardedHintButton);
            rewardedHintState = CreateLabel(page, "Демо: реклама не подключена", 18, Teal, Anchor.BottomLeft, new Vector2(220, 108), new Vector2(400, 28), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateStoreCard(page, "Без рекламы", "Спокойное путешествие по музеям", Teal, new Vector2(0, -60), "Подписка", null);
            CreateStoreCard(page, "Коллекционер", "Дополнительные истории и альбомы", Coral, new Vector2(520, -60), "Скоро", null);
        }

        private void BuildProfile()
        {
            RectTransform page = CreatePage(Page.Profile, "Profile");
            BuildHeader(page, "Профиль");
            RectTransform card = CreatePanel(page, "ProfileCard", new Color(1f, 1f, 1f, 0.9f), Anchor.Center, new Vector2(0, -20), new Vector2(1320, 600));
            CreateLabel(card, "АМЕЛИ · студентка искусства", 24, Teal, Anchor.Top, new Vector2(0, -48), new Vector2(720, 34), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, "Твой музейный дневник", 54, Ink, Anchor.Top, new Vector2(0, -105), new Vector2(930, 68), TextAlignmentOptions.Center, FontStyles.Bold);
            profileFoundCount = CreateProfileMetric(card, "00", "AI-следов\nнайдено", -350);
            CreateProfileMetric(card, "01", "открытый\nмузей", 0);
            profileCollectionCount = CreateProfileMetric(card, "00", "серий\nсобрано", 350);
            CreateLabel(card, "Прогресс сохраняется на этом устройстве автоматически.", 22, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.Bottom, new Vector2(0, 62), new Vector2(1050, 34), TextAlignmentOptions.Center);
        }

        private void OpenDailyBonus()
        {
            // The gift remains inspectable after claiming; Show refreshes the UTC-day state.
            Show(Page.DailyBonus);
        }

        private void ClaimDailyBonus()
        {
            if (dailyBonusClaimed)
            {
                Show(Page.Home);
                return;
            }

            dailyBonusClaimed = true;
            dailyClaimUtc = DateTime.UtcNow.ToString("yyyy-MM-dd");
            hintCount++;
            SaveProgress();
            UpdateProgressLabels();
            Show(Page.Home);
        }

        private void ClaimRewardedHint()
        {
            if (rewardedHintClaimed)
            {
                return;
            }

            rewardedHintClaimed = true;
            hintCount++;
            SaveProgress();
            UpdateProgressLabels();
        }

        private void OpenChapter(int chapterIndex)
        {
            if (!IsChapterUnlocked(chapterIndex))
            {
                return;
            }

            TickAttemptClock(); ClearTemporaryMarks();
            selectedChapter = chapterIndex;
            EnsureAttempt();
            clockStamp=Time.realtimeSinceStartupAsDouble;
            SaveProgress();
            UpdateGalleryContent();
            Show(IsChapterComplete(chapterIndex) ? Page.Found : Page.Gallery);
        }

        private void UseHintForCurrentChapter()
        {
            UseBonusBrush(0);
        }

        private void OpenFoundForCurrentChapter()
        {
            if (IsChapterComplete(selectedChapter))
            {
                UpdateFoundContent();
                Show(Page.Found);
            }
        }

        private void CollectCurrentChapterAndOpenCollection()
        {
            if (!IsChapterComplete(selectedChapter))
            {
                Show(Page.Gallery);
                return;
            }

            chapterCollected[selectedChapter] = true;
            SaveProgress();
            UpdateProgressLabels();
            Show(Page.Collection);
        }

        private void CollectCurrentChapterAndOpenNextChapter()
        {
            if (!IsChapterComplete(selectedChapter))
            {
                Show(Page.Gallery);
                return;
            }

            chapterCollected[selectedChapter] = true;
            SaveProgress();
            UpdateProgressLabels();

            int nextChapter = GetNextUncollectedChapter();
            if (nextChapter >= 0)
            {
                OpenChapter(nextChapter);
            }
            else
            {
                Show(Page.Collection);
            }
        }

        private void UpdateProgressLabels()
        {
            int foundCount = FoundChapterCount;
            int foundArtifacts = 0;
            for (int i = 0; i < Chapters.Length; i++) foundArtifacts += FoundArtifactCount(i);
            if (profileFoundCount != null) profileFoundCount.text = foundArtifacts.ToString("00");
            if (profileCollectionCount != null) profileCollectionCount.text = (foundCount == Chapters.Length ? 1 : 0).ToString("00");
            if (collectionProgress != null)
            {
                collectionProgress.text = $"{foundCount} из {Chapters.Length} картин в учебной главе";
            }

            if (mapProgress != null)
            {
                mapProgress.text = $"Открыто: 1 из 10 музеев · {foundCount} из {Chapters.Length} картин";
            }

            foreach (TextMeshProUGUI bonusLabel in discoveryBonusLabels)
            {
                bonusLabel.text = $"К {foundCount}";
            }

            foreach (TextMeshProUGUI bonusLabel in hintBonusLabels)
            {
                bonusLabel.text = $"П {hintCount}";
            }

            UpdateChapterCards();
            UpdateCollectionCards();
            UpdateCollectionNextAction();
            UpdateDailyBonusLabels();
            UpdateRewardedHintState();
            UpdateGalleryContent();
            UpdateFoundContent();
        }

        private void UpdateChapterCards()
        {
            for (int i = 0; i < chapterCards.Count; i++)
            {
                bool unlocked = IsChapterUnlocked(i);
                bool collected = chapterCollected[i];
                ChapterCardView view = chapterCards[i];
                view.Card.color = unlocked ? Color.white : new Color(0.94f, 0.88f, 0.78f, 0.88f);
                view.Frame.color = unlocked ? Gold : Sand;
                view.Art.sprite = unlocked ? GetChapterArtwork(i) : whiteSprite;
                view.Art.color = unlocked ? Color.white : new Color(0f, 0f, 0f, 0f);
                view.Number.text = $"{i + 1:00}";
                view.Number.color = collected ? Teal : unlocked ? Coral : Ink * new Color(1f, 1f, 1f, 0.45f);
                view.Subtitle.text = collected ? "Картина восстановлена" : unlocked ? Chapters[i].Objective : $"Откроется после картины {i:00}";
                view.Button.interactable = unlocked;
                view.Button.image.color = unlocked ? Teal : new Color(0.55f, 0.60f, 0.58f);
                view.ButtonLabel.text = collected ? "Пройдено" : unlocked ? "Играть" : "Закрыто";
            }
        }

        private void UpdateCollectionCards()
        {
            if (myGallery != null) myGallery.Refresh();
            if (museumGallery != null) museumGallery.Refresh();
            for (int i = 0; i < collectionCards.Count; i++)
            {
                bool collected = chapterCollected[i];
                CollectionCardView view = collectionCards[i];
                view.Frame.color = collected ? Gold : Sand;
                view.Art.sprite = collected ? GetChapterArtwork(i) : whiteSprite;
                view.Art.color = collected ? Color.white : new Color(0f, 0f, 0f, 0f);
                view.State.text = collected ? "ВОССТАНОВЛЕНА" : "НЕ НАЙДЕНА";
                view.State.color = collected ? Teal : Ink * new Color(1f, 1f, 1f, 0.45f);
                view.Hint.text = collected ? Chapters[i].Fact : "Продолжай путешествие";
            }
        }

        private void UpdateCollectionNextAction()
        {
            if (collectionNextButton == null)
            {
                return;
            }

            int nextChapter = GetNextUncollectedChapter();
            bool hasNextChapter = nextChapter >= 0;
            collectionNextButton.gameObject.SetActive(hasNextChapter);
            if (museumGallery != null && museumGallery.nextPainting != null)
                museumGallery.nextPainting.gameObject.SetActive(hasNextChapter);
            if (hasNextChapter && collectionNextButtonLabel != null)
            {
                collectionNextButtonLabel.text = "Следующая картина";
            }
        }

        private int GetNextUncollectedChapter()
        {
            for (int i = 0; i < Chapters.Length; i++)
            {
                if (!chapterCollected[i] && IsChapterUnlocked(i))
                {
                    return i;
                }
            }

            return -1;
        }

        private void OpenNextUncollectedChapterFromCollection()
        {
            int nextChapter = GetNextUncollectedChapter();
            if (nextChapter >= 0)
            {
                OpenChapter(nextChapter);
            }
        }

        private void UpdateDailyBonusLabels()
        {
            if (illustratedHome != null)
            {
                illustratedHome.SetBonusClaimed(dailyBonusClaimed);
            }
            if (dailyHomeState != null)
            {
                dailyHomeState.text = dailyBonusClaimed ? "Ежедневный бонус получен" : "Ежедневный бонус: +1 подсказка";
            }

            if (dailyHomeButton != null)
            {
                dailyHomeButton.interactable = !dailyBonusClaimed;
                dailyHomeButton.image.color = dailyBonusClaimed ? Sand : Gold;
            }

            if (dailyHomeButtonLabel != null)
            {
                dailyHomeButtonLabel.text = dailyBonusClaimed ? "Завтра снова" : "Забрать";
            }

            if (dailyClaimButton != null)
            {
                dailyClaimButton.interactable = !dailyBonusClaimed;
                dailyClaimButton.image.color = dailyBonusClaimed ? Sand : Teal;
            }

            if (dailyClaimButtonLabel != null)
            {
                dailyClaimButtonLabel.text = dailyBonusClaimed ? "Бонус получен" : "Забрать бонус";
            }
        }

        private void UpdateRewardedHintState()
        {
            if (rewardedHintButton != null)
            {
                rewardedHintButton.interactable = !rewardedHintClaimed;
            }

            if (rewardedHintButtonLabel != null)
            {
                rewardedHintButtonLabel.text = rewardedHintClaimed ? "Награда получена" : "Смотреть рекламу";
            }

            if (rewardedHintState != null)
            {
                rewardedHintState.text = rewardedHintClaimed ? "Демо-награда: +1 подсказка выдана" : "Демо: реклама не подключена";
            }
        }

        private void UpdateGalleryContent()
        {
            if (galleryTitle == null)
            {
                return;
            }

            ChapterData chapter = Chapters[selectedChapter];
            bool collected = chapterCollected[selectedChapter] && !replaying[selectedChapter];
            bool hintUsed = hintUsedForChapter[selectedChapter];
            Sprite artwork = GetChapterArtwork(selectedChapter);
            if (galleryPainting != null)
            {
                bool artworkChanged = galleryPainting.sprite != artwork;
                galleryPainting.sprite = artwork;
                if (artworkChanged && galleryPanZoom != null)
                {
                    galleryPanZoom.SetArtworkSize(CalculateArtworkDisplaySize(artwork));
                }
            }

            int foundArtifacts = FoundArtifactCount(selectedChapter);
            int totalArtifacts = chapter.Artifacts.Length;
            bool allArtifactsFound = IsChapterComplete(selectedChapter);
            galleryTitle.text = collected
                ? $"{chapter.Title} восстановлена"
                : allArtifactsFound ? $"{chapter.Title}: все дорисовки найдены" : $"{chapter.Title}: найди AI-дорисовки";
            galleryTargetProgress.text = $"НАЙДЕНО {foundArtifacts} / {totalArtifacts}";
            galleryDescription.text = collected
                ? "Эта картина уже в коллекции. Вернись в музей, чтобы открыть следующий зал."
                : allArtifactsFound ? "Все чужеродные детали найдены. Картина готова к восстановлению." : $"{chapter.Objective} Увеличивай картину и изучай детали.";
            if(AirtistApprovedTheme.Current!=null)
            {
                galleryTitle.text=chapter.Title;
                galleryDescription.text="Найди лишние детали";
            }
            galleryFeedback.text = collected
                ? "Эта работа уже восстановлена."
                : allArtifactsFound ? "Все AI-дорисовки найдены!" : hintUsed ? "Подсказка мягко выделила область, которую стоит рассмотреть." : $"Найди чужие предметы: {foundArtifacts} из {totalArtifacts}. Зум поможет рассмотреть детали.";
            galleryHint.text = overviewMode?"Обзор: зум двумя пальцами или колесом мыши. Таймер продолжает идти."
                : ActiveHint(exactHintTargets[selectedChapter])?"Тапни по выделенному предмету: проверка бесплатна."
                : ActiveHint(areaHintTargets[selectedChapter])?"Предмет находится в отмеченной области."
                : "Для движения и зума выбери «Обзор».";
            UpdateArtifactTargets(collected, hintUsed);
            galleryHintButton.interactable = !adPending;
            galleryHintButtonLabel.text = gameplayScreen != null ? "?" : hintUsed ? "Подсказка дана" : hintCount > 0 ? "Подсказка" : "Нет подсказок";
            RefreshAttemptHud();
        }

        private void RegisterIncorrectTap()
        {
            if(!TrySpendCheck()) return;
            if (galleryFeedback != null && (!chapterCollected[selectedChapter] || replaying[selectedChapter]))
            {
                galleryFeedback.text = "Пока нет. Ищи то, что выглядит слишком современным или чужим для стиля художника.";
            }
            SaveProgress(); RefreshAttemptHud();
        }

        private void FindArtworkArtifact(int chapterIndex, int artifactIndex)
        {
            if (chapterIndex != selectedChapter || (chapterCollected[selectedChapter] && !replaying[selectedChapter]) || artifactFound[chapterIndex][artifactIndex])
            {
                return;
            }

            if(!TrySpendCheck()) return;
            artifactFound[chapterIndex][artifactIndex] = true;
            AwardFirstCompletion();
            SaveProgress();
            UpdateProgressLabels();

            if (IsChapterComplete(selectedChapter))
            {
                OpenFoundForCurrentChapter();
                return;
            }

            ArtifactData artifact = Chapters[chapterIndex].Artifacts[artifactIndex];
            int remaining = Chapters[chapterIndex].Artifacts.Length - FoundArtifactCount(chapterIndex);
            galleryFeedback.text = $"Найдено: {artifact.Name}. Осталось дорисовок: {remaining}.";
        }

        private void UpdateArtifactTargets(bool paintingCollected, bool hintUsed)
        {
            int highlightedArtifact = exactHintTargets[selectedChapter]-1;
            for (int i = 0; i < galleryArtifactTargets.Count; i++)
            {
                ArtifactTargetView target = galleryArtifactTargets[i];
                bool visible = target.ChapterIndex == selectedChapter
                    && !paintingCollected
                    && !artifactFound[target.ChapterIndex][target.ArtifactIndex];
                target.Root.SetActive(visible);
                target.HitArea.color = visible && target.ArtifactIndex == highlightedArtifact
                    ? new Color(Gold.r, Gold.g, Gold.b, 0.34f)
                    : new Color(0f, 0f, 0f, 0f);
            }
            RefreshAreaHintVisual();
        }

        private ArtifactTargetView CreateArtifactTarget(RectTransform parent, int chapterIndex, int artifactIndex, ArtifactData artifact)
        {
            GameObject targetObject = new GameObject($"ArtifactTarget{chapterIndex + 1}_{artifactIndex + 1}", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            targetObject.transform.SetParent(parent, false);
            RectTransform target = targetObject.GetComponent<RectTransform>();
            target.anchorMin = artifact.NormalizedPosition;
            target.anchorMax = artifact.NormalizedPosition;
            target.pivot = new Vector2(0.5f, 0.5f);
            target.anchoredPosition = Vector2.zero;
            target.sizeDelta = artifact.HitSize;

            UnityEngine.UI.Image hitArea = targetObject.GetComponent<UnityEngine.UI.Image>();
            hitArea.sprite = roundedSprite;
            hitArea.type = UnityEngine.UI.Image.Type.Sliced;
            hitArea.color = new Color(0f, 0f, 0f, 0f);
            hitArea.raycastTarget = true;

            UnityEngine.UI.Button button = targetObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = hitArea;
            button.transition = UnityEngine.UI.Selectable.Transition.None;
            if(gameplayScreen!=null)
                targetObject.AddComponent<AirtistArtworkPointer>().clicked=e=>HandleArtworkPointer(e,chapterIndex,artifactIndex);
            else button.onClick.AddListener(() => FindArtworkArtifact(chapterIndex, artifactIndex));

            if (CreateIllustratedArtifact(target,parent,chapterIndex,artifactIndex)) { }
            else if (artifact.Visual == ArtifactVisual.Moustache)
            {
                RectTransform leftCurl = CreatePanel(target, "MoustacheLeft", new Color(0.16f, 0.09f, 0.06f, 0.9f), Anchor.Center, new Vector2(-28f, 1f), new Vector2(62f, 13f));
                leftCurl.localRotation = Quaternion.Euler(0f, 0f, -18f);
                leftCurl.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                RectTransform rightCurl = CreatePanel(target, "MoustacheRight", new Color(0.16f, 0.09f, 0.06f, 0.9f), Anchor.Center, new Vector2(28f, 1f), new Vector2(62f, 13f));
                rightCurl.localRotation = Quaternion.Euler(0f, 0f, 18f);
                rightCurl.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            }
            else if (artifact.Visual == ArtifactVisual.Badge)
            {
                RectTransform badge = CreatePanel(target, "AnachronisticBadge", new Color(0.25f, 0.92f, 0.84f, 0.96f), Anchor.Center, Vector2.zero, new Vector2(58f, 58f));
                badge.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                CreateLabel(badge, "AI", 18, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(44f, 30f), TextAlignmentOptions.Center, FontStyles.Bold);
            }
            else if (artifact.Visual == ArtifactVisual.Signal)
            {
                RectTransform signal = CreatePanel(target, "AlienHorizonSignal", Coral, Anchor.Center, Vector2.zero, new Vector2(74f, 44f));
                signal.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                CreateLabel(signal, "AI", 17, Cream, Anchor.Center, Vector2.zero, new Vector2(56f, 28f), TextAlignmentOptions.Center, FontStyles.Bold);
            }
            else
            {
                RectTransform tag = CreatePanel(target, "DigitalTimeTag", new Color(0.93f, 0.85f, 0.38f, 0.96f), Anchor.Center, Vector2.zero, new Vector2(70f, 36f));
                tag.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
                CreateLabel(tag, "2030", 14, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(60f, 24f), TextAlignmentOptions.Center, FontStyles.Bold);
            }

            targetObject.SetActive(false);
            return new ArtifactTargetView(targetObject, hitArea, chapterIndex, artifactIndex);
        }

        private Vector2 CalculateArtworkDisplaySize(Sprite artwork)
        {
            float maximumWidth = gameplayScreen != null ? Mathf.Max(1, gameplayScreen.viewport.rect.width - 8) : 930f;
            float maximumHeight = gameplayScreen != null ? Mathf.Max(1, gameplayScreen.viewport.rect.height - 8) : 600f;
            if (artwork == null || artwork.rect.height <= 0f)
            {
                return new Vector2(maximumWidth, maximumHeight);
            }

            float artworkAspect = artwork.rect.width / artwork.rect.height;
            float frameAspect = maximumWidth / maximumHeight;
            return artworkAspect >= frameAspect
                ? new Vector2(maximumWidth, maximumWidth / artworkAspect)
                : new Vector2(maximumHeight * artworkAspect, maximumHeight);
        }

        private static bool[][] CreateArtifactState()
        {
            bool[][] state = new bool[Chapters.Length][];
            for (int chapterIndex = 0; chapterIndex < Chapters.Length; chapterIndex++)
            {
                state[chapterIndex] = new bool[Chapters[chapterIndex].Artifacts.Length];
            }

            return state;
        }

        private int FoundArtifactCount(int chapterIndex)
        {
            int found = 0;
            for (int artifactIndex = 0; artifactIndex < artifactFound[chapterIndex].Length; artifactIndex++)
            {
                if (artifactFound[chapterIndex][artifactIndex])
                {
                    found++;
                }
            }

            return found;
        }

        private bool IsChapterComplete(int chapterIndex)
        {
            return FoundArtifactCount(chapterIndex) == Chapters[chapterIndex].Artifacts.Length;
        }

        private int GetNextUnfoundArtifactIndex(int chapterIndex)
        {
            for (int artifactIndex = 0; artifactIndex < artifactFound[chapterIndex].Length; artifactIndex++)
            {
                if (!artifactFound[chapterIndex][artifactIndex])
                {
                    return artifactIndex;
                }
            }

            return -1;
        }

        private Sprite GetChapterArtwork(int chapterIndex)
        {
            if (louvrePaintings != null && chapterIndex >= 0 && chapterIndex < louvrePaintings.Length && louvrePaintings[chapterIndex] != null)
            {
                return louvrePaintings[chapterIndex];
            }

            return whiteSprite;
        }

        private void UpdateFoundContent()
        {
            if(resultHeading!=null) RefreshRestorationScreen();
        }

        private int FoundChapterCount
        {
            get
            {
                int count = 0;
                foreach (bool collected in chapterCollected)
                {
                    if (collected)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private bool IsChapterUnlocked(int chapterIndex)
        {
            if(chapterIndex<0 || chapterIndex>=Chapters.Length) return false;
            for(int i=0;i<chapterIndex;i++)
                if(!chapterCollected[i] || (!IsChapterComplete(i) && !replaying[i])) return false;
            return true;
        }

        private void Show(Page target)
        {
            if(adPending)return;
            if(energyShop!=null)energyShop.gameObject.SetActive(false);
            ChangeAttemptPage(target);
            if (target == Page.Profile && myGalleryPrefab != null) target = Page.Collection;
            UpdateFullscreenBackground(target);
            if (target == Page.Collection && myGallery != null) myGallery.Refresh();
            if (target == Page.Museum && museumGallery != null) museumGallery.Refresh();
            if (universalClose != null) universalClose.SetActive(target != Page.Home);
            if (universalHome != null) universalHome.SetActive(target != Page.Home);
            if (universalHeader != null) universalHeader.SetAsLastSibling();
            dailyBonusClaimed = dailyClaimUtc == DateTime.UtcNow.ToString("yyyy-MM-dd");
            UpdateDailyBonusLabels();
            if (target == Page.Museum)
            {
                UpdateChapterCards();
            }
            else if (target == Page.Gallery)
            {
                UpdateGalleryContent();
            }
            else if (target == Page.Found)
            {
                UpdateFoundContent();
            }

            foreach (KeyValuePair<Page, GameObject> pair in pages)
            {
                pair.Value.SetActive(pair.Key == target);
            }
            BringDeveloperResetToFront();
        }

        private RectTransform CreatePage(Page page, string objectName)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            Stretch(rect);
            UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = whiteSprite;
            image.color = page == Page.Gallery ? new Color(0.18f, 0.32f, 0.34f, 1f) : Cream;
            image.raycastTarget = false;
            pages.Add(page, go);
            return rect;
        }

        private void BuildHeader(RectTransform page, string location)
        {
            if (homeScreenPrefab != null) return;
            // Every screen uses the same framed navigation as Home, so Home is always one tap away.
            RectTransform headerFrame = CreatePanel(page, "HeaderFrame", AntiqueGold, Anchor.Top, new Vector2(0, -54), new Vector2(1760, 100));
            RectTransform header = CreatePanel(headerFrame, "Header", new Color(Parchment.r, Parchment.g, Parchment.b, 0.96f), Anchor.Center, Vector2.zero, new Vector2(1740, 80));
            CreateLabel(header, "AIrtist", 45, Ink, Anchor.Left, new Vector2(52, 0), new Vector2(220, 54), TextAlignmentOptions.Left, FontStyles.Bold | FontStyles.Italic);
            CreateLabel(header, location, 18, Ink * new Color(1f, 1f, 1f, 0.73f), Anchor.Left, new Vector2(292, 0), new Vector2(360, 30), TextAlignmentOptions.Left);
            CreateBonusTray(header);
            CreateFramedButton(header, "Главная", Parchment, DeepTeal, Anchor.Right, new Vector2(-602, 0), new Vector2(114, 58), () => Show(Page.Home), 14);
            CreateFramedButton(header, "Карта", Parchment, DeepTeal, Anchor.Right, new Vector2(-470, 0), new Vector2(112, 58), () => Show(Page.WorldMap), 16);
            CreateFramedButton(header, "Коллекция", Parchment, DeepTeal, Anchor.Right, new Vector2(-330, 0), new Vector2(140, 58), () => Show(Page.Collection), 15);
            CreateFramedButton(header, "Магазин", Parchment, DeepTeal, Anchor.Right, new Vector2(-184, 0), new Vector2(120, 58), () => Show(Page.Store), 15);
            CreateFramedButton(header, "Профиль", Parchment, DeepTeal, Anchor.Right, new Vector2(-54, 0), new Vector2(108, 58), () => Show(Page.Profile), 15);
        }

        private void CreateBonusTray(RectTransform header)
        {
            RectTransform trayFrame = CreatePanel(header, "BonusTrayFrame", AntiqueGold, Anchor.Right, new Vector2(-784, 0), new Vector2(154, 58));
            RectTransform tray = CreatePanel(trayFrame, "BonusTray", Gold, Anchor.Center, Vector2.zero, new Vector2(146, 50));
            CreateLabel(tray, "БОНУСЫ", 11, DeepTeal, Anchor.Left, new Vector2(12, 0), new Vector2(54, 22), TextAlignmentOptions.Left, FontStyles.Bold);

            RectTransform discovery = CreatePanel(tray, "DiscoveryBonus", Parchment, Anchor.Right, new Vector2(-58, 0), new Vector2(42, 34));
            TextMeshProUGUI discoveryLabel = CreateLabel(discovery, "К 0", 12, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(38, 24), TextAlignmentOptions.Center, FontStyles.Bold);
            discoveryBonusLabels.Add(discoveryLabel);

            RectTransform hint = CreatePanel(tray, "HintBonus", Parchment, Anchor.Right, new Vector2(-10, 0), new Vector2(42, 34));
            TextMeshProUGUI hintLabel = CreateLabel(hint, "П 3", 12, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(38, 24), TextAlignmentOptions.Center, FontStyles.Bold);
            hintBonusLabels.Add(hintLabel);
        }

        private void CreateMuseumNode(RectTransform parent, string title, string city, Vector2 position, bool unlocked, Action click)
        {
            Color nodeColor = unlocked ? Coral : new Color(0.45f, 0.60f, 0.59f, 1f);
            RectTransform node = CreatePanel(parent, title.Replace(" ", string.Empty), nodeColor, Anchor.Center, position, new Vector2(270, 130));
            CreateLabel(node, unlocked ? "ОТКРЫТ" : "СКОРО", 15, unlocked ? Cream : new Color(1f, 1f, 1f, 0.72f), Anchor.Top, new Vector2(0, -24), new Vector2(180, 24), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(node, title, 23, Cream, Anchor.Center, new Vector2(0, 4), new Vector2(235, 34), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(node, city, 18, Cream * new Color(1f, 1f, 1f, 0.82f), Anchor.Bottom, new Vector2(0, 19), new Vector2(180, 25), TextAlignmentOptions.Center);
            if (unlocked)
            {
                UnityEngine.UI.Button button = node.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = node.GetComponent<UnityEngine.UI.Image>();
                button.onClick.AddListener(() => click?.Invoke());
            }
        }

        private void CreateChapterCard(RectTransform parent, int chapterIndex, Vector2 position)
        {
            ChapterData chapter = Chapters[chapterIndex];
            RectTransform card = CreateJourneyPanel(parent, "Chapter" + (chapterIndex + 1), Anchor.Center, position, new Vector2(480, 430));
            TextMeshProUGUI number = CreateLabel(card, (chapterIndex + 1).ToString("00"), 29, Coral, Anchor.TopLeft, new Vector2(38, -34), new Vector2(80, 40), TextAlignmentOptions.Left, FontStyles.Bold);
            RectTransform frame = CreatePanel(card, "MiniFrame", Gold, Anchor.Top, new Vector2(0, -75), new Vector2(272, 155));
            RectTransform art = CreateImage(card, "MiniArtwork", GetChapterArtwork(chapterIndex), Color.white, Anchor.Top, new Vector2(0, -87), new Vector2(242, 131), true);
            int titleSize = chapter.Title.Length > 19 ? 23 : 30;
            CreateLabel(card, chapter.Title, titleSize, Ink, Anchor.Top, new Vector2(0, -245), new Vector2(400, 42), TextAlignmentOptions.Center, FontStyles.Bold);
            TextMeshProUGUI subtitle = CreateLabel(card, chapter.Objective, 20, Ink * new Color(1f, 1f, 1f, 0.66f), Anchor.Top, new Vector2(0, -287), new Vector2(400, 40), TextAlignmentOptions.Center);
            UnityEngine.UI.Button button = CreateFramedButton(card, "Играть", Teal, Cream, Anchor.Bottom, new Vector2(0, 36), new Vector2(230, 58), () => OpenChapter(chapterIndex), 19);
            chapterCards.Add(new ChapterCardView
            {
                Card = card.GetComponent<UnityEngine.UI.Image>(),
                Frame = frame.GetComponent<UnityEngine.UI.Image>(),
                Art = art.GetComponent<UnityEngine.UI.Image>(),
                Number = number,
                Subtitle = subtitle,
                Button = button,
                ButtonLabel = GetButtonLabel(button)
            });
        }

        private void CreateCollectionCard(RectTransform parent, int chapterIndex, Vector2 position)
        {
            ChapterData chapter = Chapters[chapterIndex];
            RectTransform card = CreatePanel(parent, chapter.Title.Replace(" ", string.Empty), new Color(1f, 1f, 1f, 0.88f), Anchor.Center, position, new Vector2(460, 560));
            RectTransform frame = CreatePanel(card, "Frame", Sand, Anchor.Top, new Vector2(0, -22), new Vector2(270, 210));
            RectTransform art = CreateImage(card, "Art", null, new Color(0f, 0f, 0f, 0f), Anchor.Top, new Vector2(0, -39), new Vector2(236, 176), true);
            TextMeshProUGUI state = CreateLabel(card, "НЕ НАЙДЕНА", 16, Ink * new Color(1f, 1f, 1f, 0.45f), Anchor.Top, new Vector2(0, -244), new Vector2(300, 26), TextAlignmentOptions.Center, FontStyles.Bold);
            int titleSize = chapter.Title.Length > 19 ? 22 : 28;
            CreateLabel(card, chapter.Title, titleSize, Ink, Anchor.Top, new Vector2(0, -266), new Vector2(400, 50), TextAlignmentOptions.Center, FontStyles.Bold);
            TextMeshProUGUI hint = CreateLabel(card, "Продолжай путешествие", 20, Ink, Anchor.Top, new Vector2(0, -370), new Vector2(400, 165), TextAlignmentOptions.Center);
            var review = card.gameObject.AddComponent<UnityEngine.UI.Button>();
            review.targetGraphic = card.GetComponent<UnityEngine.UI.Image>();
            review.onClick.AddListener(() => { if (chapterCollected[chapterIndex]) { selectedChapter = chapterIndex; Show(Page.Found); } });
            collectionCards.Add(new CollectionCardView
            {
                Frame = frame.GetComponent<UnityEngine.UI.Image>(),
                Art = art.GetComponent<UnityEngine.UI.Image>(),
                State = state,
                Hint = hint
            });
        }

        private UnityEngine.UI.Button CreateStoreCard(RectTransform parent, string title, string subtitle, Color accent, Vector2 position, string cta, Action click)
        {
            RectTransform card = CreatePanel(parent, title.Replace(" ", string.Empty), new Color(1f, 1f, 1f, 0.9f), Anchor.Center, position, new Vector2(420, 430));
            CreatePanel(card, "Accent", accent, Anchor.Top, new Vector2(0, -10), new Vector2(420, 20));
            CreateLabel(card, title, 34, Ink, Anchor.Top, new Vector2(0, -76), new Vector2(350, 46), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, subtitle, 22, Ink * new Color(1f, 1f, 1f, 0.68f), Anchor.Top, new Vector2(0, -142), new Vector2(340, 60), TextAlignmentOptions.Center);
            CreateLabel(card, title == "Без рекламы" ? "Подписка на месяц" : title == "Подсказка" ? "Награда за просмотр" : "Будущая серия", 20, Teal, Anchor.Center, new Vector2(0, -58), new Vector2(320, 30), TextAlignmentOptions.Center, FontStyles.Bold);
            return CreateButton(card, cta, accent, accent == Gold ? DeepTeal : Cream, Anchor.Bottom, new Vector2(0, 36), new Vector2(280, 62), click, 19);
        }

        private TextMeshProUGUI CreateProfileMetric(RectTransform parent, string value, string label, float x)
        {
            var metric = CreateLabel(parent, value, 58, Coral, Anchor.Center, new Vector2(x, -32), new Vector2(210, 70), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(parent, label, 20, Ink * new Color(1f, 1f, 1f, 0.68f), Anchor.Center, new Vector2(x, -91), new Vector2(210, 55), TextAlignmentOptions.Center);
            return metric;
        }

        private RectTransform CreatePanel(RectTransform parent, string objectName, Color color, Anchor anchor, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            SetAnchor(rect, anchor, position, size);
            UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = roundedSprite;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = true;
            return rect;
        }

        private RectTransform CreateImage(RectTransform parent, string objectName, Sprite sprite, Color color, Anchor anchor, Vector2 position, Vector2 size, bool preserveAspect)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            SetAnchor(rect, anchor, position, size);
            UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite ?? whiteSprite;
            image.color = color;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = false;
            return rect;
        }

        private TextMeshProUGUI CreateLabel(RectTransform parent, string value, float size, Color color, Anchor anchor, Vector2 position, Vector2 dimensions, TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal)
        {
            GameObject go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            SetAnchor(rect, anchor, position, dimensions);
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = value;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.fontStyle = style;
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            return label;
        }

        private UnityEngine.UI.Button CreateFramedButton(RectTransform parent, string caption, Color fill, Color textColor, Anchor anchor, Vector2 position, Vector2 size, Action click, float fontSize)
        {
            RectTransform frame = CreatePanel(parent, caption.Replace(" ", string.Empty) + "Frame", AntiqueGold, anchor, position, size);
            return CreateButton(frame, caption, fill, textColor, Anchor.Center, Vector2.zero, size - new Vector2(8f, 8f), click, fontSize);
        }

        private UnityEngine.UI.Button CreateButton(RectTransform parent, string caption, Color fill, Color textColor, Anchor anchor, Vector2 position, Vector2 size, Action click, float fontSize = 22)
        {
            RectTransform rect = CreatePanel(parent, caption.Replace(" ", string.Empty) + "Button", fill, anchor, position, size);
            UnityEngine.UI.Button button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();
            // Extend the vertical touch area without changing the illustrated button footprint.
            float touchPadding = Mathf.Max(0, (132f - size.y) * .5f);
            rect.GetComponent<UnityEngine.UI.Image>().raycastPadding = new Vector4(0, -touchPadding, 0, -touchPadding);
            button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = Color.white;
            button.colors = colors;
            if (click != null)
            {
                button.onClick.AddListener(() => click());
            }

            var buttonLabel = CreateLabel(rect, caption, fontSize, textColor, Anchor.Center, Vector2.zero, size - new Vector2(18, 10), TextAlignmentOptions.Center, FontStyles.Bold);
            string navKey = caption switch { "Главная" => "nav.home", "Карта" => "nav.map", "Коллекция" => "nav.collection", "Магазин" => "nav.store", "Профиль" => "nav.profile", _ => null };
            if (navKey != null)
            {
                var localized = buttonLabel.gameObject.AddComponent<UnityEngine.Localization.Components.LocalizeStringEvent>();
                localized.StringReference = new UnityEngine.Localization.LocalizedString("AIrtistUI", navKey);
                localized.OnUpdateString.AddListener(value => buttonLabel.text = value);
                localized.RefreshString();
            }
            return button;
        }

        private static TextMeshProUGUI GetButtonLabel(UnityEngine.UI.Button button)
        {
            return button != null ? button.GetComponentInChildren<TextMeshProUGUI>() : null;
        }

        private static Sprite CreateRoundedSprite()
        {
            const int textureSize = 64;
            const float radius = 14f;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "AirtistRoundedPanel"
            };

            Color[] pixels = new Color[textureSize * textureSize];
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float dx = Mathf.Max(radius - x, 0f, x - (textureSize - 1 - radius));
                    float dy = Mathf.Max(radius - y, 0f, y - (textureSize - 1 - radius));
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius + 1f - distance);
                    pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color color);
            return color;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void SetAnchor(RectTransform rect, Anchor anchor, Vector2 position, Vector2 size)
        {
            if (anchor == Anchor.Stretch)
            {
                Stretch(rect);
                return;
            }

            Vector2 point = anchor switch
            {
                Anchor.TopLeft => new Vector2(0f, 1f),
                Anchor.Top => new Vector2(0.5f, 1f),
                Anchor.TopRight => new Vector2(1f, 1f),
                Anchor.Left => new Vector2(0f, 0.5f),
                Anchor.Center => new Vector2(0.5f, 0.5f),
                Anchor.Right => new Vector2(1f, 0.5f),
                Anchor.BottomLeft => new Vector2(0f, 0f),
                Anchor.Bottom => new Vector2(0.5f, 0f),
                Anchor.BottomRight => new Vector2(1f, 0f),
                _ => new Vector2(0.5f, 0.5f)
            };

            rect.anchorMin = point;
            rect.anchorMax = point;
            rect.pivot = point;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private enum Anchor
        {
            TopLeft,
            Top,
            TopRight,
            Left,
            Center,
            Right,
            BottomLeft,
            Bottom,
            BottomRight,
            Stretch
        }

        private readonly struct ArtifactTargetView
        {
            public readonly GameObject Root;
            public readonly UnityEngine.UI.Image HitArea;
            public readonly int ChapterIndex;
            public readonly int ArtifactIndex;

            public ArtifactTargetView(GameObject root, UnityEngine.UI.Image hitArea, int chapterIndex, int artifactIndex)
            {
                Root = root;
                HitArea = hitArea;
                ChapterIndex = chapterIndex;
                ArtifactIndex = artifactIndex;
            }
        }

        private enum ArtifactVisual
        {
            Moustache,
            Badge,
            Signal,
            TimeTag
        }

        private readonly struct ArtifactData
        {
            public readonly string Name;
            public readonly string Hint;
            public readonly Vector2 NormalizedPosition;
            public readonly Vector2 HitSize;
            public readonly ArtifactVisual Visual;

            public ArtifactData(string name, string hint, Vector2 normalizedPosition, Vector2 hitSize, ArtifactVisual visual)
            {
                Name = name;
                Hint = hint;
                NormalizedPosition = normalizedPosition;
                HitSize = hitSize;
                Visual = visual;
            }
        }

        private readonly struct ChapterData
        {
            public readonly string Title;
            public readonly string Objective;
            public readonly string FoundDescription;
            public readonly string Fact;
            public readonly string Hint;
            public readonly ArtifactData[] Artifacts;

            public ChapterData(string title, string objective, string foundDescription, string fact, string hint, ArtifactData[] artifacts)
            {
                Title = title;
                Objective = objective;
                FoundDescription = foundDescription;
                Fact = fact;
                Hint = hint;
                Artifacts = artifacts ?? Array.Empty<ArtifactData>();
            }
        }

        private sealed class ChapterCardView
        {
            public UnityEngine.UI.Image Card;
            public UnityEngine.UI.Image Frame;
            public UnityEngine.UI.Image Art;
            public TextMeshProUGUI Number;
            public TextMeshProUGUI Subtitle;
            public UnityEngine.UI.Button Button;
            public TextMeshProUGUI ButtonLabel;
        }

        private sealed class CollectionCardView
        {
            public UnityEngine.UI.Image Frame;
            public UnityEngine.UI.Image Art;
            public TextMeshProUGUI State;
            public TextMeshProUGUI Hint;
        }
    }
}
