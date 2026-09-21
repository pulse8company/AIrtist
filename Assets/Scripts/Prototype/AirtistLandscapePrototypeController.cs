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
    public sealed class AirtistLandscapePrototypeController : MonoBehaviour
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
            Profile
        }

        private static readonly Color Ink = Hex("20303F");
        private static readonly Color Cream = Hex("FFF4DF");
        private static readonly Color Teal = Hex("367C7D");
        private static readonly Color DeepTeal = Hex("1E5055");
        private static readonly Color Gold = Hex("F2B84A");
        private static readonly Color Coral = Hex("F87964");
        private static readonly Color Sand = Hex("E8D6BD");
        private static readonly Color Moss = Hex("7BAA73");
        private static readonly ChapterData[] Chapters =
        {
            new ChapterData(
                "Мона Лиза",
                "Найди 3 AI-дорисовки",
                "Чужой усик нарушал едва заметные переходы лица. Ты вернула портрету его подлинное выражение.",
                "Лувр показывает «Мону Лизу» Леонардо да Винчи в зале 711. Её мягкие переходы светотени называют сфумато.",
                "Начни с лица: у сфумато нет жёстких, чужеродных контуров.",
                new[]
                {
                    new ArtifactData("AI-усик", "Проверь лицо: у сфумато нет жёстких, чужеродных контуров.", new Vector2(0.50f, 0.68f), new Vector2(132f, 68f), ArtifactVisual.Moustache),
                    new ArtifactData("цифровая печать", "Посмотри на складки одежды: там не должно быть современной маркировки.", new Vector2(0.73f, 0.23f), new Vector2(86f, 60f), ArtifactVisual.TimeTag),
                    new ArtifactData("чужая метка", "Проверь далёкий пейзаж за фигурой.", new Vector2(0.20f, 0.46f), new Vector2(84f, 60f), ArtifactVisual.Badge)
                }),
            new ChapterData(
                "Свобода, ведущая народ",
                "Найди 3 AI-дорисовки",
                "Современный значок выбивался из исторической сцены. Ты вернула картине её драматический ритм.",
                "Делакруа написал картину после Июльской революции 1830 года. В Лувре она находится в зале 700, Salle Mollien.",
                "Ищи современные символы там, где художник использовал только исторические детали.",
                new[]
                {
                    new ArtifactData("неоновый значок", "Посмотри на центральную фигуру: яркий цифровой цвет выбивается из палитры.", new Vector2(0.48f, 0.61f), new Vector2(92f, 92f), ArtifactVisual.Badge),
                    new ArtifactData("стикер 2030", "Проверь левый край композиции.", new Vector2(0.18f, 0.43f), new Vector2(86f, 60f), ArtifactVisual.TimeTag),
                    new ArtifactData("AI-сигнал", "Проверь правую часть полотна — там спрятана ещё одна современная деталь.", new Vector2(0.79f, 0.38f), new Vector2(88f, 60f), ArtifactVisual.Signal)
                }),
            new ChapterData(
                "Плот «Медузы»",
                "Найди 3 AI-дорисовки",
                "На горизонте появился предмет, которого здесь быть не должно. Ты вернула сцене напряжение и надежду.",
                "Жерико показал эту картину на Салоне 1819 года: она рассказывает о крушении фрегата «Медуза», после которого выжили пятнадцать человек.",
                "Начни с горизонта: жесты фигур ведут к дальнему силуэту.",
                new[]
                {
                    new ArtifactData("AI-сигнал", "Проследи за жестами фигур: взгляд ведёт к дальнему силуэту на горизонте.", new Vector2(0.75f, 0.72f), new Vector2(104f, 78f), ArtifactVisual.Signal),
                    new ArtifactData("QR-метка", "Проверь фигуры слева: там не должно быть цифрового знака.", new Vector2(0.34f, 0.38f), new Vector2(86f, 60f), ArtifactVisual.Badge),
                    new ArtifactData("цифровая подпись", "Посмотри в небо — там прячется последняя дорисовка.", new Vector2(0.55f, 0.20f), new Vector2(88f, 60f), ArtifactVisual.TimeTag)
                })
        };

        [SerializeField] private Sprite homeBackground;
        [SerializeField] private Sprite galleryBackground;
        [SerializeField] private Sprite portrait;
        [SerializeField] private Sprite[] louvrePaintings;
        [SerializeField] private TMP_FontAsset font;

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
        private TextMeshProUGUI foundNumber;
        private TextMeshProUGUI foundTitle;
        private TextMeshProUGUI foundDescription;
        private TextMeshProUGUI foundFact;
        private TextMeshProUGUI foundReward;
        private TextMeshProUGUI foundRewardCaption;
        private UnityEngine.UI.Button foundCollectionButton;
        private UnityEngine.UI.Button foundNextButton;
        private TextMeshProUGUI foundNextButtonLabel;
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
            BuildInterface();
            Show(Page.Home);
        }

        private void BuildInterface()
        {
            whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
            roundedSprite = CreateRoundedSprite();

            BuildHome();
            BuildWorldMap();
            BuildMuseum();
            BuildGallery();
            BuildFound();
            BuildDailyBonus();
            BuildCollection();
            BuildStore();
            BuildProfile();
            UpdateProgressLabels();
        }

        private void BuildHome()
        {
            RectTransform page = CreatePage(Page.Home, "Home");
            CreateImage(page, "HomeBackdrop", homeBackground != null ? homeBackground : galleryBackground, Color.white, Anchor.Stretch, Vector2.zero, Vector2.zero, false);
            CreatePanel(page, "HomeWarmthVeil", new Color(1f, 0.95f, 0.83f, 0.17f), Anchor.Stretch, Vector2.zero, Vector2.zero);
            BuildHeader(page, "Париж · первая выставка");

            RectTransform headlinePlate = CreatePanel(page, "HomeTitlePlate", new Color(1f, 0.97f, 0.89f, 0.84f), Anchor.TopLeft, new Vector2(72, -144), new Vector2(840, 222));
            CreatePanel(headlinePlate, "ChapterPill", Teal, Anchor.TopLeft, new Vector2(28, -26), new Vector2(190, 34));
            CreateLabel(headlinePlate, "ГЛАВА 1 · ЛУВР", 15, Cream, Anchor.TopLeft, new Vector2(42, -31), new Vector2(160, 24), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(headlinePlate, "Старинные картины.\nСовременные следы.", 52, Ink, Anchor.TopLeft, new Vector2(28, -68), new Vector2(780, 112), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(headlinePlate, "Найди AI-дорисовки и верни картинам их историю.", 18, Ink * new Color(1f, 1f, 1f, 0.76f), Anchor.TopLeft, new Vector2(30, -190), new Vector2(740, 26), TextAlignmentOptions.Left);

            RectTransform routeCard = CreatePanel(page, "TodayRoute", new Color(1f, 0.98f, 0.91f, 0.91f), Anchor.BottomLeft, new Vector2(76, 62), new Vector2(850, 292));
            RectTransform routeThumbnailFrame = CreatePanel(routeCard, "RouteThumbnailFrame", Gold, Anchor.TopLeft, new Vector2(28, -32), new Vector2(214, 140));
            CreateImage(routeThumbnailFrame, "RouteThumbnail", GetChapterArtwork(0), Color.white, Anchor.Center, Vector2.zero, new Vector2(194, 120), true);
            CreateLabel(routeCard, "СЕГОДНЯШНИЙ МАРШРУТ", 17, Teal, Anchor.TopLeft, new Vector2(268, -32), new Vector2(360, 28), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(routeCard, "Музей у Сены", 36, Ink, Anchor.TopLeft, new Vector2(266, -74), new Vector2(440, 48), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(routeCard, "3 картины · 9 AI-следов · 1 коллекция", 19, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.TopLeft, new Vector2(268, -124), new Vector2(470, 28), TextAlignmentOptions.Left);
            CreateButton(routeCard, "Продолжить поиск", Coral, Cream, Anchor.BottomLeft, new Vector2(28, 26), new Vector2(352, 72), () => Show(Page.Museum));
            RectTransform dailyBonusPill = CreatePanel(routeCard, "DailyBonusPill", new Color(Gold.r, Gold.g, Gold.b, 0.91f), Anchor.BottomRight, new Vector2(-26, 101), new Vector2(312, 44));
            dailyHomeState = CreateLabel(dailyBonusPill, "Ежедневный бонус: +1 подсказка", 15, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(284, 26), TextAlignmentOptions.Center, FontStyles.Bold);
            dailyHomeButton = CreateButton(routeCard, "Забрать", Gold, DeepTeal, Anchor.BottomRight, new Vector2(-26, 26), new Vector2(216, 66), OpenDailyBonus, 18);
            dailyHomeButtonLabel = GetButtonLabel(dailyHomeButton);

            RectTransform artCard = CreatePanel(page, "AmelieCard", Gold, Anchor.Right, new Vector2(-96, 8), new Vector2(548, 624));
            RectTransform artInner = CreatePanel(artCard, "AmeliePortraitMat", new Color(1f, 0.97f, 0.89f, 0.96f), Anchor.Center, new Vector2(0, -10), new Vector2(510, 555));
            CreateLabel(artCard, "АМЕЛИ\nстудентка живописи", 22, DeepTeal, Anchor.TopLeft, new Vector2(28, -28), new Vector2(290, 58), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateImage(artInner, "AmeliePortrait", portrait, Color.white, Anchor.Bottom, new Vector2(0, 14), new Vector2(478, 488), true);
            CreatePanel(artCard, "RolePill", Cream, Anchor.BottomRight, new Vector2(-26, 24), new Vector2(205, 54));
            CreateLabel(artCard, "ИСКАТЕЛЬНИЦА", 15, DeepTeal, Anchor.BottomRight, new Vector2(-38, 39), new Vector2(180, 24), TextAlignmentOptions.Center, FontStyles.Bold);

            CreateButton(page, "Открыть карту мира", Teal, Cream, Anchor.BottomRight, new Vector2(-690, 70), new Vector2(350, 72), () => Show(Page.WorldMap));
        }

        private void BuildWorldMap()
        {
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
            BuildHeader(page, "Музей у Сены · Париж");
            CreateLabel(page, "Глава 1. След в галерее", 47, Ink, Anchor.TopLeft, new Vector2(84, -164), new Vector2(720, 62), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(page, "Собирай карточки картин и открывай короткие истории о художниках.", 23, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.TopLeft, new Vector2(86, -226), new Vector2(900, 34), TextAlignmentOptions.Left);

            CreateChapterCard(page, 0, new Vector2(-560, -82));
            CreateChapterCard(page, 1, new Vector2(0, -82));
            CreateChapterCard(page, 2, new Vector2(560, -82));

            RectTransform fact = CreatePanel(page, "MuseumFact", new Color(1f, 1f, 1f, 0.76f), Anchor.Bottom, new Vector2(0, 82), new Vector2(1450, 118));
            CreateLabel(fact, "ФАКТ", 17, Coral, Anchor.Left, new Vector2(42, 0), new Vector2(90, 34), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(fact, "Музеи — это не только коллекции: архитектура и способ показа искусства тоже становятся частью истории.", 22, Ink, Anchor.Left, new Vector2(154, 0), new Vector2(1180, 62), TextAlignmentOptions.Left);
        }

        private void BuildGallery()
        {
            RectTransform page = CreatePage(Page.Gallery, "Gallery");
            CreateImage(page, "GalleryBackdrop", galleryBackground, new Color(0.24f, 0.36f, 0.39f, 1f), Anchor.Stretch, Vector2.zero, Vector2.zero, false);
            CreatePanel(page, "GalleryVeil", new Color(0.08f, 0.18f, 0.21f, 0.38f), Anchor.Stretch, Vector2.zero, Vector2.zero);
            BuildHeader(page, "Учебная глава · 3 картины");
            galleryTitle = CreateLabel(page, "Найди AI-дорисовку", 39, Cream, Anchor.TopLeft, new Vector2(84, -160), new Vector2(620, 52), TextAlignmentOptions.Left, FontStyles.Bold);
            galleryTargetProgress = CreateLabel(page, "НАЙДЕНО 0 / 3", 20, Gold, Anchor.TopRight, new Vector2(-84, -160), new Vector2(250, 40), TextAlignmentOptions.Right, FontStyles.Bold);
            galleryDescription = CreateLabel(page, "Смотри на картину внимательно: инородная деталь часто прячется на самом видном месте.", 21, Cream * new Color(1f, 1f, 1f, 0.83f), Anchor.TopLeft, new Vector2(86, -214), new Vector2(960, 32), TextAlignmentOptions.Left);
            galleryFeedback = CreateLabel(page, "Тапни по детали, которая выглядит чужой для этой картины.", 19, Gold, Anchor.TopLeft, new Vector2(86, -256), new Vector2(960, 30), TextAlignmentOptions.Left, FontStyles.Bold);

            RectTransform frame = CreatePanel(page, "GoldFrame", Gold, Anchor.Center, new Vector2(0, -20), new Vector2(620, 690));
            RectTransform viewport = CreatePanel(frame, "ArtworkViewport", DeepTeal, Anchor.Center, Vector2.zero, new Vector2(570, 640));
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
            CreateButton(page, "−", Sand, DeepTeal, Anchor.BottomRight, new Vector2(-380, 46), new Vector2(64, 58), galleryPanZoom.ZoomOut, 30);
            CreateButton(page, "+", Sand, DeepTeal, Anchor.BottomRight, new Vector2(-302, 46), new Vector2(64, 58), galleryPanZoom.ZoomIn, 30);
            galleryHintButton = CreateButton(page, "Подсказка", Gold, DeepTeal, Anchor.BottomRight, new Vector2(-80, 46), new Vector2(230, 58), UseHintForCurrentChapter, 18);
            galleryHintButtonLabel = GetButtonLabel(galleryHintButton);
        }

        private void BuildFound()
        {
            RectTransform page = CreatePage(Page.Found, "Found");
            BuildHeader(page, "Находка!");
            RectTransform card = CreatePanel(page, "FoundCard", new Color(1f, 1f, 1f, 0.92f), Anchor.Center, new Vector2(0, -24), new Vector2(1280, 690));
            foundNumber = CreateLabel(card, "НАХОДКА №01", 21, Coral, Anchor.Top, new Vector2(0, -50), new Vector2(380, 32), TextAlignmentOptions.Center, FontStyles.Bold);
            foundTitle = CreateLabel(card, "AI-дорисовка обнаружена", 55, Ink, Anchor.Top, new Vector2(0, -111), new Vector2(990, 70), TextAlignmentOptions.Center, FontStyles.Bold);
            foundDescription = CreateLabel(card, string.Empty, 26, Ink * new Color(1f, 1f, 1f, 0.73f), Anchor.Top, new Vector2(0, -202), new Vector2(980, 72), TextAlignmentOptions.Center);

            RectTransform fact = CreatePanel(card, "ArtistFact", Sand, Anchor.Center, new Vector2(-230, -62), new Vector2(560, 245));
            CreateLabel(fact, "КАРТОЧКА ФАКТА", 17, Teal, Anchor.TopLeft, new Vector2(30, -28), new Vector2(300, 28), TextAlignmentOptions.Left, FontStyles.Bold);
            foundFact = CreateLabel(fact, string.Empty, 21, Ink, Anchor.Center, new Vector2(0, -14), new Vector2(486, 126), TextAlignmentOptions.Center);

            CreatePanel(card, "Reward", Gold, Anchor.Center, new Vector2(330, -62), new Vector2(245, 245));
            foundReward = CreateLabel(card, "+1", 62, DeepTeal, Anchor.Center, new Vector2(330, -36), new Vector2(180, 72), TextAlignmentOptions.Center, FontStyles.Bold);
            foundRewardCaption = CreateLabel(card, "картина в\nколлекцию", 20, DeepTeal, Anchor.Center, new Vector2(330, -104), new Vector2(180, 62), TextAlignmentOptions.Center, FontStyles.Bold);
            foundCollectionButton = CreateButton(card, "В коллекцию", Teal, Cream, Anchor.Bottom, new Vector2(-210, 48), new Vector2(340, 75), CollectCurrentChapterAndOpenCollection);
            foundNextButton = CreateButton(card, "Следующая картина", Coral, Cream, Anchor.Bottom, new Vector2(210, 48), new Vector2(340, 75), CollectCurrentChapterAndOpenNextChapter);
            foundNextButtonLabel = GetButtonLabel(foundNextButton);
        }

        private void BuildDailyBonus()
        {
            RectTransform page = CreatePage(Page.DailyBonus, "DailyBonus");
            BuildHeader(page, "Ежедневный маршрут");
            RectTransform card = CreatePanel(page, "DailyBonusCard", new Color(1f, 1f, 1f, 0.92f), Anchor.Center, new Vector2(0, -14), new Vector2(970, 590));
            CreateLabel(card, "ЕЖЕДНЕВНЫЙ БОНУС", 22, Coral, Anchor.Top, new Vector2(0, -62), new Vector2(480, 34), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, "Небольшая помощь\nдля внимательного взгляда", 48, Ink, Anchor.Top, new Vector2(0, -136), new Vector2(800, 118), TextAlignmentOptions.Center, FontStyles.Bold);
            CreatePanel(card, "DailyReward", Gold, Anchor.Center, new Vector2(0, -18), new Vector2(230, 170));
            CreateLabel(card, "+1", 58, DeepTeal, Anchor.Center, new Vector2(0, 2), new Vector2(200, 72), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, "подсказка", 21, DeepTeal, Anchor.Center, new Vector2(0, -58), new Vector2(200, 32), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, "В прототипе награда выдаётся один раз за запуск.\nСерверный календарь появится позднее.", 20, Ink * new Color(1f, 1f, 1f, 0.68f), Anchor.Bottom, new Vector2(0, 126), new Vector2(700, 58), TextAlignmentOptions.Center);
            dailyClaimButton = CreateButton(card, "Забрать бонус", Teal, Cream, Anchor.Bottom, new Vector2(0, 42), new Vector2(340, 70), ClaimDailyBonus);
            dailyClaimButtonLabel = GetButtonLabel(dailyClaimButton);
        }

        private void BuildCollection()
        {
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
            CreateProfileMetric(card, "01", "найденный\nAI-след", -350);
            CreateProfileMetric(card, "01", "открытый\nмузей", 0);
            CreateProfileMetric(card, "00", "серий\nсобрано", 350);
            CreateLabel(card, "В следующей версии здесь появятся сохранение прогресса, настройки и доступ к коллекционным альбомам.", 22, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.Bottom, new Vector2(0, 62), new Vector2(1050, 34), TextAlignmentOptions.Center);
        }

        private void OpenDailyBonus()
        {
            if (!dailyBonusClaimed)
            {
                Show(Page.DailyBonus);
            }
        }

        private void ClaimDailyBonus()
        {
            if (dailyBonusClaimed)
            {
                Show(Page.Home);
                return;
            }

            dailyBonusClaimed = true;
            hintCount++;
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
            UpdateProgressLabels();
        }

        private void OpenChapter(int chapterIndex)
        {
            if (!IsChapterUnlocked(chapterIndex))
            {
                return;
            }

            selectedChapter = chapterIndex;
            UpdateGalleryContent();
            Show(Page.Gallery);
        }

        private void UseHintForCurrentChapter()
        {
            if (chapterCollected[selectedChapter] || IsChapterComplete(selectedChapter) || hintUsedForChapter[selectedChapter] || hintCount <= 0)
            {
                return;
            }

            hintCount--;
            hintUsedForChapter[selectedChapter] = true;
            UpdateProgressLabels();
        }

        private void OpenFoundForCurrentChapter()
        {
            if (!chapterCollected[selectedChapter] && IsChapterComplete(selectedChapter))
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
            UpdateProgressLabels();

            int nextChapter = selectedChapter + 1;
            if (nextChapter < Chapters.Length)
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
                view.Card.color = unlocked ? new Color(1f, 1f, 1f, 0.88f) : new Color(0.94f, 0.88f, 0.78f, 0.76f);
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
            for (int i = 0; i < collectionCards.Count; i++)
            {
                bool collected = chapterCollected[i];
                CollectionCardView view = collectionCards[i];
                view.Frame.color = collected ? Gold : Sand;
                view.Art.sprite = collected ? GetChapterArtwork(i) : whiteSprite;
                view.Art.color = collected ? Color.white : new Color(0f, 0f, 0f, 0f);
                view.State.text = collected ? "ВОССТАНОВЛЕНА" : "НЕ НАЙДЕНА";
                view.State.color = collected ? Teal : Ink * new Color(1f, 1f, 1f, 0.45f);
                view.Hint.text = collected ? "Факт разблокирован" : "Продолжай путешествие";
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
            if (hasNextChapter && collectionNextButtonLabel != null)
            {
                collectionNextButtonLabel.text = $"К картине {nextChapter + 1:00}";
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
            bool collected = chapterCollected[selectedChapter];
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
            galleryFeedback.text = collected
                ? "Эта работа уже восстановлена."
                : allArtifactsFound ? "Все три AI-дорисовки найдены!" : hintUsed ? "Подсказка мягко выделила область, которую стоит рассмотреть." : "Тапни по детали, которая выглядит чужой для этой картины.";
            galleryHint.text = hintUsed ? $"Подсказка: {chapter.Hint}" : "Увеличивай картину кнопками + / − и перетаскивай её пальцем или мышью.";
            UpdateArtifactTargets(collected, hintUsed);
            galleryHintButton.interactable = !collected && !allArtifactsFound && !hintUsed && hintCount > 0;
            galleryHintButtonLabel.text = hintUsed ? "Подсказка дана" : hintCount > 0 ? "Подсказка" : "Нет подсказок";
        }

        private void RegisterIncorrectTap()
        {
            if (galleryFeedback != null && !chapterCollected[selectedChapter])
            {
                galleryFeedback.text = "Пока нет. Ищи то, что выглядит слишком современным или чужим для стиля художника.";
            }
        }

        private void FindArtworkArtifact(int chapterIndex, int artifactIndex)
        {
            if (chapterIndex != selectedChapter || chapterCollected[selectedChapter] || artifactFound[chapterIndex][artifactIndex])
            {
                return;
            }

            artifactFound[chapterIndex][artifactIndex] = true;
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
            int highlightedArtifact = hintUsed ? GetNextUnfoundArtifactIndex(selectedChapter) : -1;
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
            button.onClick.AddListener(() => FindArtworkArtifact(chapterIndex, artifactIndex));

            if (artifact.Visual == ArtifactVisual.Moustache)
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

        private static Vector2 CalculateArtworkDisplaySize(Sprite artwork)
        {
            const float maximumWidth = 530f;
            const float maximumHeight = 600f;
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
            if (foundNumber == null)
            {
                return;
            }

            ChapterData chapter = Chapters[selectedChapter];
            foundNumber.text = $"НАХОДКА №{selectedChapter + 1:00}";
            foundTitle.text = $"{chapter.Title}: AI-дорисовка обнаружена";
            foundDescription.text = chapter.FoundDescription;
            foundFact.text = chapter.Fact;
            foundReward.text = "+1";
            foundRewardCaption.text = $"картина\n{FoundChapterCount + (chapterCollected[selectedChapter] ? 0 : 1)} из {Chapters.Length}";

            bool hasNextChapter = selectedChapter + 1 < Chapters.Length;
            if (foundNextButton != null)
            {
                foundNextButton.gameObject.SetActive(hasNextChapter);
            }

            if (foundNextButtonLabel != null && hasNextChapter)
            {
                foundNextButtonLabel.text = $"К картине {selectedChapter + 2:00}";
            }

            if (foundCollectionButton != null)
            {
                RectTransform collectionButtonRect = foundCollectionButton.GetComponent<RectTransform>();
                collectionButtonRect.anchoredPosition = hasNextChapter ? new Vector2(-210, 48) : new Vector2(0, 48);
            }
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
            return chapterIndex == 0 || chapterCollected[chapterIndex - 1];
        }

        private void Show(Page target)
        {
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
            RectTransform header = CreatePanel(page, "Header", new Color(1f, 0.96f, 0.87f, 0.96f), Anchor.Top, new Vector2(0, -42), new Vector2(1920, 86));
            CreateLabel(header, "AIrtist", 37, DeepTeal, Anchor.Left, new Vector2(56, 0), new Vector2(180, 50), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(header, location, 20, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.Left, new Vector2(250, 0), new Vector2(500, 32), TextAlignmentOptions.Left);
            CreateBonusTray(header);
            CreateButton(header, "Карта", Teal, Cream, Anchor.Right, new Vector2(-466, 0), new Vector2(120, 48), () => Show(Page.WorldMap), 18);
            CreateButton(header, "Коллекция", Teal, Cream, Anchor.Right, new Vector2(-325, 0), new Vector2(140, 48), () => Show(Page.Collection), 18);
            CreateButton(header, "Магазин", Gold, DeepTeal, Anchor.Right, new Vector2(-184, 0), new Vector2(118, 48), () => Show(Page.Store), 18);
            CreateButton(header, "Профиль", Coral, Cream, Anchor.Right, new Vector2(-62, 0), new Vector2(100, 48), () => Show(Page.Profile), 18);
        }

        private void CreateBonusTray(RectTransform header)
        {
            RectTransform tray = CreatePanel(header, "BonusTray", new Color(0.89f, 0.84f, 0.74f, 0.96f), Anchor.Right, new Vector2(-602, 0), new Vector2(255, 48));
            CreateLabel(tray, "БОНУСЫ", 11, DeepTeal, Anchor.Left, new Vector2(16, 0), new Vector2(68, 22), TextAlignmentOptions.Left, FontStyles.Bold);

            RectTransform discovery = CreatePanel(tray, "DiscoveryBonus", Cream, Anchor.Right, new Vector2(-89, 0), new Vector2(67, 34));
            TextMeshProUGUI discoveryLabel = CreateLabel(discovery, "К 0", 17, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(58, 24), TextAlignmentOptions.Center, FontStyles.Bold);
            discoveryBonusLabels.Add(discoveryLabel);

            RectTransform hint = CreatePanel(tray, "HintBonus", Gold, Anchor.Right, new Vector2(-14, 0), new Vector2(67, 34));
            TextMeshProUGUI hintLabel = CreateLabel(hint, "П 3", 17, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(58, 24), TextAlignmentOptions.Center, FontStyles.Bold);
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
            RectTransform card = CreatePanel(parent, "Chapter" + (chapterIndex + 1), new Color(1f, 1f, 1f, 0.88f), Anchor.Center, position, new Vector2(480, 430));
            TextMeshProUGUI number = CreateLabel(card, (chapterIndex + 1).ToString("00"), 29, Coral, Anchor.TopLeft, new Vector2(38, -34), new Vector2(80, 40), TextAlignmentOptions.Left, FontStyles.Bold);
            RectTransform frame = CreatePanel(card, "MiniFrame", Gold, Anchor.Top, new Vector2(0, -150), new Vector2(272, 185));
            RectTransform art = CreateImage(card, "MiniArtwork", GetChapterArtwork(chapterIndex), Color.white, Anchor.Top, new Vector2(0, -150), new Vector2(242, 154), true);
            int titleSize = chapter.Title.Length > 19 ? 23 : 30;
            CreateLabel(card, chapter.Title, titleSize, Ink, Anchor.Center, new Vector2(0, -70), new Vector2(400, 48), TextAlignmentOptions.Center, FontStyles.Bold);
            TextMeshProUGUI subtitle = CreateLabel(card, chapter.Objective, 20, Ink * new Color(1f, 1f, 1f, 0.66f), Anchor.Center, new Vector2(0, -116), new Vector2(400, 52), TextAlignmentOptions.Center);
            UnityEngine.UI.Button button = CreateButton(card, "Играть", Teal, Cream, Anchor.Bottom, new Vector2(0, 36), new Vector2(230, 58), () => OpenChapter(chapterIndex), 19);
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
            RectTransform card = CreatePanel(parent, chapter.Title.Replace(" ", string.Empty), new Color(1f, 1f, 1f, 0.88f), Anchor.Center, position, new Vector2(430, 440));
            RectTransform frame = CreatePanel(card, "Frame", Sand, Anchor.Top, new Vector2(0, -92), new Vector2(270, 210));
            RectTransform art = CreateImage(card, "Art", null, new Color(0f, 0f, 0f, 0f), Anchor.Top, new Vector2(0, -92), new Vector2(236, 176), true);
            TextMeshProUGUI state = CreateLabel(card, "НЕ НАЙДЕНА", 16, Ink * new Color(1f, 1f, 1f, 0.45f), Anchor.Center, new Vector2(0, -112), new Vector2(300, 26), TextAlignmentOptions.Center, FontStyles.Bold);
            int titleSize = chapter.Title.Length > 19 ? 22 : 28;
            CreateLabel(card, chapter.Title, titleSize, Ink, Anchor.Center, new Vector2(0, -151), new Vector2(360, 45), TextAlignmentOptions.Center, FontStyles.Bold);
            TextMeshProUGUI hint = CreateLabel(card, "Продолжай путешествие", 19, Ink * new Color(1f, 1f, 1f, 0.64f), Anchor.Center, new Vector2(0, -191), new Vector2(340, 28), TextAlignmentOptions.Center);
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

        private void CreateProfileMetric(RectTransform parent, string value, string label, float x)
        {
            CreateLabel(parent, value, 58, Coral, Anchor.Center, new Vector2(x, -32), new Vector2(210, 70), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(parent, label, 20, Ink * new Color(1f, 1f, 1f, 0.68f), Anchor.Center, new Vector2(x, -91), new Vector2(210, 55), TextAlignmentOptions.Center);
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

        private UnityEngine.UI.Button CreateButton(RectTransform parent, string caption, Color fill, Color textColor, Anchor anchor, Vector2 position, Vector2 size, Action click, float fontSize = 22)
        {
            RectTransform rect = CreatePanel(parent, caption.Replace(" ", string.Empty) + "Button", fill, anchor, position, size);
            UnityEngine.UI.Button button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();
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

            CreateLabel(rect, caption, fontSize, textColor, Anchor.Center, Vector2.zero, size - new Vector2(18, 10), TextAlignmentOptions.Center, FontStyles.Bold);
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
