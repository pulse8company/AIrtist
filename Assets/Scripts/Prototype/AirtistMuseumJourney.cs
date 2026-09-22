using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        [SerializeField] private Sprite[] museumIllustrations = new Sprite[4];
        [SerializeField] private Sprite[] museumMapIcons = new Sprite[4];
        public void ConfigureMuseumMapIcons(Sprite[] icons) => museumMapIcons = icons;
        [SerializeField] private TMP_FontAsset journeyFont;
        [SerializeField] private Sprite journeyPanel;
        private int selectedMuseum;
        private UnityEngine.UI.Image museumHero;
        private TextMeshProUGUI museumName, museumCity, museumStatus, museumFact, museumEnterLabel;
        private UnityEngine.UI.Button museumEnter;
        private GameObject museumLock;

        private static readonly string[] MuseumNames = { "Лувр", "Метрополитен", "Эрмитаж", "Прадо" };
        private static readonly string[] MuseumCities = { "Париж · Франция", "Нью-Йорк · США", "Санкт-Петербург · Россия", "Мадрид · Испания" };
        private static readonly string[] MuseumFacts = {
            "Прежде чем стать музеем, Лувр был дворцом французских монархов. Музей открылся в 1793 году.",
            "Метрополитен основан в 1870 году. В 1880 году музей переехал на своё нынешнее место у Центрального парка.",
            "Началом коллекции Эрмитажа считается 1764 год: Екатерина II приобрела собрание картин берлинского купца Гоцковского.",
            "Здание Прадо спроектировал Хуан де Вильянуэва. Музей впервые открылся для публики в ноябре 1819 года."
        };
        // Hand-registered city locations on this illustrated, non-georeferenced map.
        // Coordinates are relative to the cropped MapContent, not to the decorative frame.
        private static readonly Vector2[] MuseumMapPoints = {
            new Vector2((764f-22)/1628, (916f-311)/810), new Vector2((445f-22)/1628, (916f-333)/810),
            new Vector2((872f-22)/1628, (916f-249)/810), new Vector2((733f-22)/1628, (916f-348)/810)
        };

        public void ConfigureMuseumJourney(Sprite[] illustrations, TMP_FontAsset displayFont, Sprite panel)
        {
            if (illustrations == null || illustrations.Length != 4) throw new ArgumentException("Four museum illustrations required.");
            museumIllustrations = illustrations;
            journeyFont = displayFont;
            journeyPanel = panel;
        }

        private Sprite MuseumArt(int index) => museumIllustrations != null && index >= 0 && index < museumIllustrations.Length
            ? museumIllustrations[index] : null;

        private void BuildMuseumPins(RectTransform content)
        {
            if (content == null) return;
            for (int i = 0; i < MuseumNames.Length; i++)
            {
                int museum = i;
                var root = new GameObject("MuseumPin_" + i, typeof(RectTransform)).GetComponent<RectTransform>();
                root.SetParent(content, false);
                root.anchorMin = root.anchorMax = MuseumMapPoints[i];
                root.anchoredPosition = Vector2.zero;
                root.sizeDelta = Vector2.zero;
                var mini = CreateImage(root, "MuseumMapIcon", museumMapIcons[i], Color.white, Anchor.Center, Vector2.zero, new Vector2(42, 42), true);
                var button = mini.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = mini.GetComponent<UnityEngine.UI.Image>();
                button.onClick.AddListener(() => OpenMuseumPreview(museum));
                mini.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
                mini.GetComponent<UnityEngine.UI.Image>().raycastPadding = new Vector4(-2, -2, -2, -2);
                if (i != 0) CreateMiniLock(mini, new Vector2(15, -13)).transform.localScale = Vector3.one * .30f;
            }
        }

        private void BuildMuseumPreview()
        {
            var page = CreatePage(Page.MuseumPreview, "MuseumPreview");
            AddJourneyBackdrop(page);
            BuildHeader(page, "Музейный маршрут");
            var card = CreateJourneyPanel(page, "MuseumPassport", Anchor.Center, new Vector2(0, -62), new Vector2(1540, 740));
            var picture = CreateJourneyPanel(card, "PictureFrame", Anchor.Left, new Vector2(42, 0), new Vector2(760, 544));
            museumHero = CreateImage(picture, "MuseumIllustration", MuseumArt(0), Color.white, Anchor.Center, Vector2.zero, new Vector2(732, 488), true).GetComponent<UnityEngine.UI.Image>();
            museumLock = CreateMiniLock(picture, new Vector2(337, -218));
            museumName = CreateLabel(card, "Лувр", 48, Ink, Anchor.TopLeft, new Vector2(842, -78), new Vector2(640, 80), TextAlignmentOptions.Left, FontStyles.Bold);
            museumCity = CreateLabel(card, MuseumCities[0], 25, Teal, Anchor.TopLeft, new Vector2(844, -168), new Vector2(610, 45), TextAlignmentOptions.Left);
            museumStatus = CreateLabel(card, "Собрано 0/3", 30, Rust, Anchor.TopLeft, new Vector2(844, -234), new Vector2(610, 48), TextAlignmentOptions.Left, FontStyles.Bold);
            var fact = CreateJourneyPanel(card, "MuseumStory", Anchor.TopLeft, new Vector2(830, -315), new Vector2(650, 224));
            CreateLabel(fact, "ИСТОРИЯ МЕСТА", 18, Teal, Anchor.TopLeft, new Vector2(25, -21), new Vector2(570, 32), TextAlignmentOptions.Left, FontStyles.Bold);
            museumFact = CreateLabel(fact, MuseumFacts[0], 23, Ink, Anchor.Center, new Vector2(0, -20), new Vector2(592, 144), TextAlignmentOptions.Left);
            museumEnter = CreateFramedButton(card, "Войти", Teal, Cream, Anchor.BottomRight, new Vector2(-60, 64), new Vector2(306, 82), EnterSelectedMuseum, 28);
            museumEnterLabel = GetButtonLabel(museumEnter);
            CreateFramedButton(card, "К карте", Gold, Ink, Anchor.BottomRight, new Vector2(-400, 64), new Vector2(260, 82), () => Show(Page.WorldMap), 25);
            ApplyJourneyTypography(page);
        }

        private void OpenMuseumPreview(int index)
        {
            if (index < 0 || index >= MuseumNames.Length) return;
            selectedMuseum = index;
            museumName.text = MuseumNames[index]; museumCity.text = MuseumCities[index];
            museumHero.sprite = MuseumArt(index); museumFact.text = MuseumFacts[index];
            bool open = index == 0;
            museumStatus.text = open ? $"Собрано {FoundChapterCount}/{Chapters.Length}" : "Новый маршрут · скоро";
            museumEnter.interactable = open;
            museumEnterLabel.text = open ? "Войти" : "Закрыто";
            museumLock.SetActive(!open);
            Show(Page.MuseumPreview);
        }

        private void EnterSelectedMuseum()
        {
            // UI state is not the permission check: never enter an unimplemented museum.
            if (selectedMuseum == 0) Show(Page.Museum);
        }

        private void AddJourneyBackdrop(RectTransform page)
        {
            CreateImage(page, "MuseumAmbience", MuseumArt(0), Color.white, Anchor.Stretch, Vector2.zero, Vector2.zero, false);
            CreatePanel(page, "ParchmentWash", new Color(.99f, .95f, .86f, .90f), Anchor.Stretch, Vector2.zero, Vector2.zero).GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
        }

        private RectTransform CreateJourneyPanel(RectTransform parent, string name, Anchor anchor, Vector2 position, Vector2 size)
        {
            if (journeyPanel != null)
            {
                var panel = CreatePanel(parent, name, Color.white, anchor, position, size);
                var image = panel.GetComponent<UnityEngine.UI.Image>();
                image.sprite = journeyPanel;
                image.pixelsPerUnitMultiplier = 6;
                image.raycastTarget = false;
                return panel;
            }
            CreatePanel(parent, name + "Shadow", new Color(.15f, .08f, .025f, .22f), anchor, position + new Vector2(0, -7), size).GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            var outer = CreatePanel(parent, name + "Gold", Hex("9B6228"), anchor, position, size);
            outer.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            var gold = CreatePanel(outer, "GoldHighlight", Hex("F5D78A"), Anchor.Stretch, Vector2.zero, Vector2.zero);
            gold.offsetMin = Vector2.one * 3; gold.offsetMax = -Vector2.one * 3;
            var inner = CreatePanel(gold, name, Parchment, Anchor.Stretch, Vector2.zero, Vector2.zero);
            inner.offsetMin = Vector2.one * 5; inner.offsetMax = -Vector2.one * 5;
            gold.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            inner.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            // Return outer rect so caller's authored dimensions are not changed by border insets.
            return outer;
        }

        private GameObject CreateMiniLock(RectTransform parent, Vector2 position)
        {
            var root = new GameObject("MiniLock", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(parent, false); SetAnchor(root, Anchor.Center, position, new Vector2(44, 48));
            CreatePanel(root, "Shackle", AntiqueGold, Anchor.Center, new Vector2(0, 10), new Vector2(27, 32));
            CreatePanel(root, "ShackleOpening", Parchment, Anchor.Center, new Vector2(0, 11), new Vector2(15, 21));
            CreatePanel(root, "LockBody", Gold, Anchor.Center, new Vector2(0, -8), new Vector2(40, 31));
            CreatePanel(root, "Keyhole", DeepTeal, Anchor.Center, new Vector2(0, -8), new Vector2(6, 12));
            foreach (var image in root.GetComponentsInChildren<UnityEngine.UI.Image>()) image.raycastTarget = false;
            return root.gameObject;
        }

        private void ApplyJourneyTypography(RectTransform root)
        {
            if (journeyFont == null) return;
            foreach (var label in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                label.font = journeyFont;
                label.enableAutoSizing = true;
                label.fontSizeMax = label.fontSize;
                label.fontSizeMin = label.fontSize * .8f;
            }
        }
    }
}
