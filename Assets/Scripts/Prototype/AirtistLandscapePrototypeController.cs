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

        [SerializeField] private Sprite galleryBackground;
        [SerializeField] private Sprite portrait;
        [SerializeField] private TMP_FontAsset font;

        private readonly Dictionary<Page, GameObject> pages = new();
        private readonly List<TextMeshProUGUI> discoveryBonusLabels = new();
        private Sprite roundedSprite;
        private Sprite whiteSprite;
        private bool clueCollected;
        private bool initialized;
        private TextMeshProUGUI collectionProgress;
        private TextMeshProUGUI mapProgress;
        private UnityEngine.UI.Image firstCollectionFrame;
        private UnityEngine.UI.Image firstCollectionArt;
        private TextMeshProUGUI firstCollectionState;
        private TextMeshProUGUI firstCollectionHint;

        public void Configure(Sprite backdrop, Sprite featuredPortrait, TMP_FontAsset uiFont)
        {
            galleryBackground = backdrop;
            portrait = featuredPortrait;
            font = uiFont;

            if (Application.isPlaying)
            {
                Initialize();
            }
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
            BuildCollection();
            BuildStore();
            BuildProfile();
        }

        private void BuildHome()
        {
            RectTransform page = CreatePage(Page.Home, "Home");
            BuildHeader(page, "Париж · первая выставка");

            CreateLabel(page, "Старинные картины.\nСовременные следы.", 64, Ink, Anchor.TopLeft, new Vector2(82, -188), new Vector2(760, 150), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(page, "На полотнах появляются детали, которых художник никогда не рисовал.\nПомоги Амели вернуть историю искусству.", 27, Ink * new Color(1f, 1f, 1f, 0.76f), Anchor.TopLeft, new Vector2(84, -356), new Vector2(700, 90), TextAlignmentOptions.Left);

            RectTransform routeCard = CreatePanel(page, "TodayRoute", new Color(1f, 1f, 1f, 0.78f), Anchor.BottomLeft, new Vector2(84, 84), new Vector2(780, 260));
            CreateLabel(routeCard, "СЕГОДНЯШНИЙ МАРШРУТ", 18, Teal, Anchor.TopLeft, new Vector2(34, -28), new Vector2(360, 32), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(routeCard, "Музей у Сены", 37, Ink, Anchor.TopLeft, new Vector2(34, -78), new Vector2(430, 52), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(routeCard, "1 новая картина · 1 AI-след", 23, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.TopLeft, new Vector2(34, -132), new Vector2(450, 34), TextAlignmentOptions.Left);
            CreateButton(routeCard, "Продолжить поиск", Coral, Cream, Anchor.BottomLeft, new Vector2(34, 30), new Vector2(330, 70), () => Show(Page.Museum));

            RectTransform artCard = CreatePanel(page, "AmelieCard", Gold, Anchor.Right, new Vector2(-150, 0), new Vector2(570, 650));
            CreateLabel(artCard, "АМЕЛИ\nстудентка живописи", 23, DeepTeal, Anchor.TopLeft, new Vector2(32, -32), new Vector2(300, 65), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateImage(artCard, "AmeliePortrait", portrait, Color.white, Anchor.Bottom, new Vector2(0, 42), new Vector2(430, 455), true);
            CreatePanel(artCard, "Sticker", Cream, Anchor.BottomRight, new Vector2(-32, 30), new Vector2(195, 62));
            CreateLabel(artCard, "ИСКАТЕЛЬНИЦА", 16, DeepTeal, Anchor.BottomRight, new Vector2(-40, 48), new Vector2(180, 28), TextAlignmentOptions.Center, FontStyles.Bold);

            CreateButton(page, "Открыть карту мира", Teal, Cream, Anchor.BottomRight, new Vector2(-760, 106), new Vector2(335, 70), () => Show(Page.WorldMap));
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

            CreateChapterCard(page, "01", "Портрет Амели", "Найди инородную деталь", new Vector2(-560, -82), true, () => Show(Page.Gallery));
            CreateChapterCard(page, "02", "Зал света", "Откроется после находки", new Vector2(0, -82), false, null);
            CreateChapterCard(page, "03", "Тайная рама", "Откроется после 5 картин", new Vector2(560, -82), false, null);

            RectTransform fact = CreatePanel(page, "MuseumFact", new Color(1f, 1f, 1f, 0.76f), Anchor.Bottom, new Vector2(0, 82), new Vector2(1450, 118));
            CreateLabel(fact, "ФАКТ", 17, Coral, Anchor.Left, new Vector2(42, 0), new Vector2(90, 34), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(fact, "Музеи — это не только коллекции: архитектура и способ показа искусства тоже становятся частью истории.", 22, Ink, Anchor.Left, new Vector2(154, 0), new Vector2(1180, 62), TextAlignmentOptions.Left);
        }

        private void BuildGallery()
        {
            RectTransform page = CreatePage(Page.Gallery, "Gallery");
            CreateImage(page, "GalleryBackdrop", galleryBackground, new Color(0.24f, 0.36f, 0.39f, 1f), Anchor.Stretch, Vector2.zero, Vector2.zero, false);
            CreatePanel(page, "GalleryVeil", new Color(0.08f, 0.18f, 0.21f, 0.38f), Anchor.Stretch, Vector2.zero, Vector2.zero);
            BuildHeader(page, "Глава 1 · Портрет Амели");
            CreateLabel(page, "Найди AI-дорисовку", 39, Cream, Anchor.TopLeft, new Vector2(84, -160), new Vector2(560, 52), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(page, "Смотри на картину внимательно: инородная деталь часто прячется на самом видном месте.", 21, Cream * new Color(1f, 1f, 1f, 0.83f), Anchor.TopLeft, new Vector2(86, -214), new Vector2(900, 32), TextAlignmentOptions.Left);

            RectTransform frame = CreatePanel(page, "GoldFrame", Gold, Anchor.Center, new Vector2(0, -20), new Vector2(620, 690));
            CreatePanel(frame, "FrameInset", DeepTeal, Anchor.Center, Vector2.zero, new Vector2(570, 640));
            CreateImage(frame, "Painting", portrait, Color.white, Anchor.Center, Vector2.zero, new Vector2(530, 600), true);

            CreateButton(frame, "✦  AI-след", Coral, Cream, Anchor.Center, new Vector2(80, -18), new Vector2(165, 58), () => Show(Page.Found));
            CreateLabel(page, "Подсказка: неестественный контур?", 21, Cream, Anchor.Bottom, new Vector2(0, 56), new Vector2(520, 32), TextAlignmentOptions.Center, FontStyles.Bold);
        }

        private void BuildFound()
        {
            RectTransform page = CreatePage(Page.Found, "Found");
            BuildHeader(page, "Находка!");
            RectTransform card = CreatePanel(page, "FoundCard", new Color(1f, 1f, 1f, 0.92f), Anchor.Center, new Vector2(0, -24), new Vector2(1280, 690));
            CreateLabel(card, "НАХОДКА №01", 21, Coral, Anchor.Top, new Vector2(0, -50), new Vector2(380, 32), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, "AI-дорисовка обнаружена", 55, Ink, Anchor.Top, new Vector2(0, -111), new Vector2(990, 70), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, "Инородный декоративный штрих не относится к композиции.\nТы вернула картине её настоящую историю.", 26, Ink * new Color(1f, 1f, 1f, 0.73f), Anchor.Top, new Vector2(0, -202), new Vector2(980, 72), TextAlignmentOptions.Center);

            RectTransform fact = CreatePanel(card, "ArtistFact", Sand, Anchor.Center, new Vector2(-230, -62), new Vector2(560, 245));
            CreateLabel(fact, "КАРТОЧКА ФАКТА", 17, Teal, Anchor.TopLeft, new Vector2(30, -28), new Vector2(300, 28), TextAlignmentOptions.Left, FontStyles.Bold);
            CreateLabel(fact, "Амели — собирательный герой. В финальной игре реальные факты будут проверяться с музейными и академическими источниками.", 21, Ink, Anchor.Center, new Vector2(0, -14), new Vector2(486, 126), TextAlignmentOptions.Center);

            CreatePanel(card, "Reward", Gold, Anchor.Center, new Vector2(330, -62), new Vector2(245, 245));
            CreateLabel(card, "+1", 62, DeepTeal, Anchor.Center, new Vector2(330, -36), new Vector2(180, 72), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, "картина в\nколлекцию", 20, DeepTeal, Anchor.Center, new Vector2(330, -104), new Vector2(180, 62), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateButton(card, "Добавить в коллекцию", Teal, Cream, Anchor.Bottom, new Vector2(0, 48), new Vector2(390, 75), CollectAndOpenCollection);
        }

        private void BuildCollection()
        {
            RectTransform page = CreatePage(Page.Collection, "Collection");
            BuildHeader(page, "Моя коллекция");
            CreateLabel(page, "Картины с возвращённой историей", 45, Ink, Anchor.TopLeft, new Vector2(84, -164), new Vector2(860, 60), TextAlignmentOptions.Left, FontStyles.Bold);
            collectionProgress = CreateLabel(page, "0 из 60 картин в этой музейной главе", 23, Ink * new Color(1f, 1f, 1f, 0.7f), Anchor.TopLeft, new Vector2(86, -226), new Vector2(620, 34), TextAlignmentOptions.Left);

            CreateCollectionCard(page, "Портрет Амели", new Vector2(-520, -42), true);
            CreateCollectionCard(page, "Зал света", new Vector2(0, -42), false);
            CreateCollectionCard(page, "Тайная рама", new Vector2(520, -42), false);
            CreateLabel(page, "В прототипе открыта одна картина. Полная игра будет добавлять факты, серии и тематические альбомы.", 22, Ink * new Color(1f, 1f, 1f, 0.72f), Anchor.Bottom, new Vector2(0, 90), new Vector2(1300, 34), TextAlignmentOptions.Center);
        }

        private void BuildStore()
        {
            RectTransform page = CreatePage(Page.Store, "Store");
            BuildHeader(page, "Магазин");
            CreateLabel(page, "Комфортный поиск, без давления", 46, Ink, Anchor.Top, new Vector2(0, -150), new Vector2(900, 62), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(page, "Это макет монетизации: реальные покупки и реклама в прототип не подключены.", 22, Ink * new Color(1f, 1f, 1f, 0.7f), Anchor.Top, new Vector2(0, -215), new Vector2(1000, 34), TextAlignmentOptions.Center);

            CreateStoreCard(page, "Подсказка", "Мягкая помощь в сложной картине", Gold, new Vector2(-520, -60), "Смотреть рекламу");
            CreateStoreCard(page, "Без рекламы", "Спокойное путешествие по музеям", Teal, new Vector2(0, -60), "Подписка");
            CreateStoreCard(page, "Коллекционер", "Дополнительные истории и альбомы", Coral, new Vector2(520, -60), "Скоро");
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

        private void CollectAndOpenCollection()
        {
            clueCollected = true;
            UpdateProgressLabels();
            Show(Page.Collection);
        }

        private void UpdateProgressLabels()
        {
            if (collectionProgress != null)
            {
                collectionProgress.text = clueCollected ? "1 из 60 картин в этой музейной главе" : "0 из 60 картин в этой музейной главе";
            }

            if (mapProgress != null)
            {
                mapProgress.text = clueCollected ? "Открыто: 1 из 10 музеев · 1 картина" : "Открыто: 1 из 10 музеев";
            }

            if (clueCollected && firstCollectionArt != null)
            {
                firstCollectionArt.sprite = portrait ?? whiteSprite;
                firstCollectionArt.color = Color.white;
                firstCollectionFrame.color = Gold;
                firstCollectionState.text = "ВОССТАНОВЛЕНА";
                firstCollectionState.color = Teal;
                firstCollectionHint.text = "Факт разблокирован";
            }

            foreach (TextMeshProUGUI bonusLabel in discoveryBonusLabels)
            {
                bonusLabel.text = clueCollected ? "★ 1" : "★ 0";
            }
        }

        private void Show(Page target)
        {
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
            TextMeshProUGUI discoveryLabel = CreateLabel(discovery, "★ 0", 17, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(58, 24), TextAlignmentOptions.Center, FontStyles.Bold);
            discoveryBonusLabels.Add(discoveryLabel);

            RectTransform hint = CreatePanel(tray, "HintBonus", Gold, Anchor.Right, new Vector2(-14, 0), new Vector2(67, 34));
            CreateLabel(hint, "✦ 3", 17, DeepTeal, Anchor.Center, Vector2.zero, new Vector2(58, 24), TextAlignmentOptions.Center, FontStyles.Bold);
        }

        private void CreateMuseumNode(RectTransform parent, string title, string city, Vector2 position, bool unlocked, Action click)
        {
            Color nodeColor = unlocked ? Coral : new Color(0.45f, 0.60f, 0.59f, 1f);
            RectTransform node = CreatePanel(parent, title.Replace(" ", string.Empty), nodeColor, Anchor.Center, position, new Vector2(270, 130));
            CreateLabel(node, unlocked ? "✦ ОТКРЫТ" : "○ СКОРО", 15, unlocked ? Cream : new Color(1f, 1f, 1f, 0.72f), Anchor.Top, new Vector2(0, -24), new Vector2(180, 24), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(node, title, 23, Cream, Anchor.Center, new Vector2(0, 4), new Vector2(235, 34), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(node, city, 18, Cream * new Color(1f, 1f, 1f, 0.82f), Anchor.Bottom, new Vector2(0, 19), new Vector2(180, 25), TextAlignmentOptions.Center);
            if (unlocked)
            {
                UnityEngine.UI.Button button = node.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = node.GetComponent<UnityEngine.UI.Image>();
                button.onClick.AddListener(() => click?.Invoke());
            }
        }

        private void CreateChapterCard(RectTransform parent, string number, string title, string subtitle, Vector2 position, bool unlocked, Action click)
        {
            RectTransform card = CreatePanel(parent, "Chapter" + number, unlocked ? new Color(1f, 1f, 1f, 0.88f) : new Color(0.94f, 0.88f, 0.78f, 0.76f), Anchor.Center, position, new Vector2(480, 430));
            CreateLabel(card, number, 29, unlocked ? Coral : Ink * new Color(1f, 1f, 1f, 0.45f), Anchor.TopLeft, new Vector2(38, -34), new Vector2(80, 40), TextAlignmentOptions.Left, FontStyles.Bold);
            CreatePanel(card, "MiniFrame", unlocked ? Gold : Sand, Anchor.Top, new Vector2(0, -150), new Vector2(272, 185));
            CreateImage(card, "MiniPortrait", unlocked ? portrait : null, unlocked ? Color.white : new Color(0f, 0f, 0f, 0f), Anchor.Top, new Vector2(0, -150), new Vector2(242, 154), true);
            CreateLabel(card, title, 30, Ink, Anchor.Center, new Vector2(0, -70), new Vector2(400, 40), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, subtitle, 20, Ink * new Color(1f, 1f, 1f, 0.66f), Anchor.Center, new Vector2(0, -116), new Vector2(400, 52), TextAlignmentOptions.Center);
            CreateButton(card, unlocked ? "Играть" : "Закрыто", unlocked ? Teal : new Color(0.55f, 0.60f, 0.58f), Cream, Anchor.Bottom, new Vector2(0, 36), new Vector2(230, 58), () => click?.Invoke(), 19);
        }

        private void CreateCollectionCard(RectTransform parent, string title, Vector2 position, bool first)
        {
            bool unlocked = first && clueCollected;
            RectTransform card = CreatePanel(parent, title.Replace(" ", string.Empty), new Color(1f, 1f, 1f, 0.88f), Anchor.Center, position, new Vector2(430, 440));
            RectTransform frame = CreatePanel(card, "Frame", unlocked ? Gold : Sand, Anchor.Top, new Vector2(0, -92), new Vector2(270, 210));
            RectTransform art = CreateImage(card, "Art", unlocked ? portrait : null, unlocked ? Color.white : new Color(0f, 0f, 0f, 0f), Anchor.Top, new Vector2(0, -92), new Vector2(236, 176), true);
            TextMeshProUGUI state = CreateLabel(card, unlocked ? "ВОССТАНОВЛЕНА" : "НЕ НАЙДЕНА", 16, unlocked ? Teal : Ink * new Color(1f, 1f, 1f, 0.45f), Anchor.Center, new Vector2(0, -112), new Vector2(300, 26), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, title, 28, Ink, Anchor.Center, new Vector2(0, -151), new Vector2(360, 38), TextAlignmentOptions.Center, FontStyles.Bold);
            TextMeshProUGUI hint = CreateLabel(card, unlocked ? "Факт разблокирован" : "Продолжай путешествие", 19, Ink * new Color(1f, 1f, 1f, 0.64f), Anchor.Center, new Vector2(0, -191), new Vector2(340, 28), TextAlignmentOptions.Center);

            if (first)
            {
                firstCollectionFrame = frame.GetComponent<UnityEngine.UI.Image>();
                firstCollectionArt = art.GetComponent<UnityEngine.UI.Image>();
                firstCollectionState = state;
                firstCollectionHint = hint;
            }
        }

        private void CreateStoreCard(RectTransform parent, string title, string subtitle, Color accent, Vector2 position, string cta)
        {
            RectTransform card = CreatePanel(parent, title.Replace(" ", string.Empty), new Color(1f, 1f, 1f, 0.9f), Anchor.Center, position, new Vector2(420, 430));
            CreatePanel(card, "Accent", accent, Anchor.Top, new Vector2(0, -10), new Vector2(420, 20));
            CreateLabel(card, title, 34, Ink, Anchor.Top, new Vector2(0, -76), new Vector2(350, 46), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateLabel(card, subtitle, 22, Ink * new Color(1f, 1f, 1f, 0.68f), Anchor.Top, new Vector2(0, -142), new Vector2(340, 60), TextAlignmentOptions.Center);
            CreateLabel(card, title == "Без рекламы" ? "Подписка на месяц" : title == "Подсказка" ? "Награда за просмотр" : "Будущая серия", 20, Teal, Anchor.Center, new Vector2(0, -58), new Vector2(320, 30), TextAlignmentOptions.Center, FontStyles.Bold);
            CreateButton(card, cta, accent, accent == Gold ? DeepTeal : Cream, Anchor.Bottom, new Vector2(0, 36), new Vector2(280, 62), null, 19);
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
    }
}
