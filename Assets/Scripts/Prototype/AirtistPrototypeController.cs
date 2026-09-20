using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    /// <summary>
    /// A self-contained vertical slice of AIrtist's hidden-object loop.
    /// The scene builder supplies the art references; this component creates the mobile UI at runtime.
    /// </summary>
    public sealed class AirtistPrototypeController : MonoBehaviour
    {
        [SerializeField] private Sprite galleryBackground;
        [SerializeField] private Sprite portrait;
        [SerializeField] private TMP_FontAsset interfaceFont;

        private Sprite whiteSprite;
        private Sprite moustacheSprite;
        private TMPro.TextMeshProUGUI objectiveLabel;
        private TMPro.TextMeshProUGUI collectionLabel;
        private GameObject suspiciousMoustache;
        private GameObject successOverlay;
        private GameObject collectionOverlay;
        private TMPro.TextMeshProUGUI collectionBody;
        private bool forgeryFound;

        public void Configure(Sprite backgroundSprite, Sprite portraitSprite, TMP_FontAsset font)
        {
            galleryBackground = backgroundSprite;
            portrait = portraitSprite;
            interfaceFont = font;
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;

            if (galleryBackground == null || portrait == null || interfaceFont == null)
            {
                Debug.LogError("[AIrtist] Prototype art or TextMeshPro font is missing.");
                return;
            }

            whiteSprite = CreateSolidSprite();
            moustacheSprite = CreateMoustacheSprite();
            BuildInterface();
        }

        private void OnDestroy()
        {
            DestroyRuntimeSprite(whiteSprite);
            DestroyRuntimeSprite(moustacheSprite);
        }

        private void BuildInterface()
        {
            var canvas = transform as RectTransform;
            if (canvas == null)
            {
                Debug.LogError("[AIrtist] The prototype root must use a RectTransform.");
                return;
            }

            var background = CreateImage("MuseumBackground", canvas, Color.white, false);
            background.sprite = galleryBackground;
            background.preserveAspect = false;
            Stretch(background.rectTransform);

            CreateHeader(canvas);
            CreateObjectiveCard(canvas);
            CreatePainting(canvas);
            CreateSuccessOverlay(canvas);
            CreateCollectionOverlay(canvas);
        }

        private void CreateHeader(RectTransform canvas)
        {
            var header = CreateImage("Header", canvas, new Color(0.04f, 0.12f, 0.18f, 0.90f), false);
            SetTopBar(header.rectTransform, 188f, 0f);

            var logo = CreateText("Logo", header.transform, "AIrtist", 58f, Color.white, TextAlignmentOptions.Left, FontStyles.Bold);
            SetTopLeft(logo.rectTransform, new Vector2(54f, -28f), new Vector2(350f, 80f));

            var museum = CreateText("Museum", header.transform, "ПАРИЖ  •  ЛУВР", 25f, new Color(0.95f, 0.80f, 0.45f), TextAlignmentOptions.Left, FontStyles.Bold);
            SetTopLeft(museum.rectTransform, new Vector2(58f, -105f), new Vector2(430f, 46f));

            var collectionButton = CreateButton("CollectionButton", header.transform, "КОЛЛЕКЦИЯ  0/1", new Color(0.12f, 0.42f, 0.45f, 0.98f), ShowCollection);
            SetTopRight(collectionButton.GetComponent<RectTransform>(), new Vector2(-42f, -48f), new Vector2(310f, 86f));
            collectionLabel = collectionButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }

        private void CreateObjectiveCard(RectTransform canvas)
        {
            var card = CreateImage("ObjectiveCard", canvas, new Color(1f, 0.94f, 0.79f, 0.97f), false);
            SetTopBar(card.rectTransform, 128f, 212f);
            card.rectTransform.offsetMin = new Vector2(34f, card.rectTransform.offsetMin.y);
            card.rectTransform.offsetMax = new Vector2(-34f, card.rectTransform.offsetMax.y);

            var eyebrow = CreateText("Eyebrow", card.transform, "ЗАДАНИЕ", 21f, new Color(0.35f, 0.18f, 0.07f), TextAlignmentOptions.Left, FontStyles.Bold);
            SetTopLeft(eyebrow.rectTransform, new Vector2(34f, -21f), new Vector2(220f, 32f));

            objectiveLabel = CreateText("Objective", card.transform, "Найдите AI-дорисовку на портрете", 30f, new Color(0.12f, 0.11f, 0.11f), TextAlignmentOptions.Left, FontStyles.Bold);
            SetTopLeft(objectiveLabel.rectTransform, new Vector2(33f, -53f), new Vector2(940f, 64f));
        }

        private void CreatePainting(RectTransform canvas)
        {
            var painting = CreateImage("PortraitOfAmelie", canvas, Color.white, false);
            painting.sprite = portrait;
            painting.preserveAspect = true;
            SetCenter(painting.rectTransform, new Vector2(0f, -54f), new Vector2(440f, 550f));

            var moustache = CreateButton("SuspiciousMoustache", painting.transform, string.Empty, Color.white, FindForgery);
            var moustacheImage = moustache.GetComponent<UnityEngine.UI.Image>();
            moustacheImage.sprite = moustacheSprite;
            moustacheImage.type = UnityEngine.UI.Image.Type.Simple;
            moustacheImage.preserveAspect = true;
            moustacheImage.color = Color.white;
            moustache.GetComponentInChildren<TMPro.TextMeshProUGUI>().gameObject.SetActive(false);

            var moustacheRect = moustache.GetComponent<RectTransform>();
            moustacheRect.anchorMin = new Vector2(0.5f, 0.625f);
            moustacheRect.anchorMax = new Vector2(0.5f, 0.625f);
            moustacheRect.pivot = new Vector2(0.5f, 0.5f);
            moustacheRect.anchoredPosition = Vector2.zero;
            moustacheRect.sizeDelta = new Vector2(142f, 54f);
            suspiciousMoustache = moustache.gameObject;

            var captionPlate = CreateImage("CaptionPlate", canvas, new Color(0.05f, 0.08f, 0.10f, 0.88f), false);
            SetCenter(captionPlate.rectTransform, new Vector2(0f, -394f), new Vector2(570f, 98f));
            var caption = CreateText("Caption", captionPlate.transform, "«Портрет Амели»\nУчебный экспонат • Галерея 01", 22f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(caption.rectTransform, 12f);
        }

        private void CreateSuccessOverlay(RectTransform canvas)
        {
            var overlay = CreateImage("SuccessOverlay", canvas, new Color(0.02f, 0.05f, 0.08f, 0.76f), true);
            Stretch(overlay.rectTransform);
            successOverlay = overlay.gameObject;

            var card = CreateImage("SuccessCard", overlay.transform, new Color(1f, 0.95f, 0.82f, 1f), false);
            SetCenter(card.rectTransform, new Vector2(0f, 0f), new Vector2(860f, 680f));

            var title = CreateText("SuccessTitle", card.transform, "ДОРИСОВКА НАЙДЕНА!", 43f, new Color(0.10f, 0.35f, 0.31f), TextAlignmentOptions.Center, FontStyles.Bold);
            SetTopBar(title.rectTransform, 78f, 54f);
            title.rectTransform.offsetMin = new Vector2(34f, title.rectTransform.offsetMin.y);
            title.rectTransform.offsetMax = new Vector2(-34f, title.rectTransform.offsetMax.y);

            var reward = CreateText("Reward", card.transform, "+1  ФРАГМЕНТ КОЛЛЕКЦИИ", 30f, new Color(0.70f, 0.38f, 0.05f), TextAlignmentOptions.Center, FontStyles.Bold);
            SetTopBar(reward.rectTransform, 52f, 155f);

            var fact = CreateText(
                "LouvreFact",
                card.transform,
                "ФАКТ О МУЗЕЕ\nЛувр открылся для публики\n10 августа 1793 года.",
                29f,
                new Color(0.12f, 0.12f, 0.12f),
                TextAlignmentOptions.Center,
                FontStyles.Normal);
            SetCenter(fact.rectTransform, new Vector2(0f, 20f), new Vector2(710f, 220f));

            var confirm = CreateButton("CollectButton", card.transform, "ДОБАВИТЬ В КОЛЛЕКЦИЮ", new Color(0.10f, 0.48f, 0.42f, 1f), CollectPainting);
            SetBottomCenter(confirm.GetComponent<RectTransform>(), new Vector2(0f, 62f), new Vector2(650f, 104f));

            successOverlay.SetActive(false);
        }

        private void CreateCollectionOverlay(RectTransform canvas)
        {
            var overlay = CreateImage("CollectionOverlay", canvas, new Color(0.02f, 0.05f, 0.08f, 0.80f), true);
            Stretch(overlay.rectTransform);
            collectionOverlay = overlay.gameObject;

            var card = CreateImage("CollectionCard", overlay.transform, new Color(0.94f, 0.97f, 0.94f, 1f), false);
            SetCenter(card.rectTransform, Vector2.zero, new Vector2(900f, 1200f));

            var title = CreateText("CollectionTitle", card.transform, "МОЯ КОЛЛЕКЦИЯ", 46f, new Color(0.07f, 0.29f, 0.30f), TextAlignmentOptions.Center, FontStyles.Bold);
            SetTopBar(title.rectTransform, 82f, 46f);

            var thumbnail = CreateImage("CollectedPortrait", card.transform, Color.white, false);
            thumbnail.sprite = portrait;
            thumbnail.preserveAspect = true;
            SetTopCenter(thumbnail.rectTransform, new Vector2(0f, -174f), new Vector2(390f, 490f));
            thumbnail.gameObject.SetActive(false);

            collectionBody = CreateText("CollectionBody", card.transform, "В коллекции пока пусто.\nНайдите первую AI-дорисовку.", 31f, new Color(0.15f, 0.15f, 0.15f), TextAlignmentOptions.Center, FontStyles.Normal);
            SetCenter(collectionBody.rectTransform, new Vector2(0f, -180f), new Vector2(720f, 180f));

            var close = CreateButton("CloseCollection", card.transform, "НАЗАД В ГАЛЕРЕЮ", new Color(0.12f, 0.42f, 0.45f, 1f), HideCollection);
            SetBottomCenter(close.GetComponent<RectTransform>(), new Vector2(0f, 58f), new Vector2(620f, 98f));

            collectionOverlay.SetActive(false);
        }

        private void FindForgery()
        {
            if (forgeryFound)
            {
                return;
            }

            forgeryFound = true;
            suspiciousMoustache.SetActive(false);
            objectiveLabel.text = "Задание выполнено • Найдено 1/1";
            successOverlay.SetActive(true);
        }

        private void CollectPainting()
        {
            successOverlay.SetActive(false);
            collectionLabel.text = "КОЛЛЕКЦИЯ  1/1";
        }

        private void ShowCollection()
        {
            var thumbnail = collectionOverlay.transform.Find("CollectionCard/CollectedPortrait");
            if (thumbnail != null)
            {
                thumbnail.gameObject.SetActive(forgeryFound);
            }

            collectionBody.text = forgeryFound
                ? "«Портрет Амели»\nФрагмент: AI-дорисовка усов\n\nЛувр открылся для публики в 1793 году."
                : "В коллекции пока пусто.\nНайдите первую AI-дорисовку.";
            collectionOverlay.SetActive(true);
        }

        private void HideCollection()
        {
            collectionOverlay.SetActive(false);
        }

        private UnityEngine.UI.Image CreateImage(string name, Transform parent, Color color, bool raycastTarget)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = whiteSprite;
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private TMPro.TextMeshProUGUI CreateText(string name, Transform parent, string value, float size, Color color, TextAlignmentOptions alignment, FontStyles style)
        {
            var rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            text.font = interfaceFont;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }

        private UnityEngine.UI.Button CreateButton(string name, Transform parent, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = whiteSprite;
            image.color = color;
            image.raycastTarget = true;

            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.93f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = Color.white;
            button.colors = colors;
            button.onClick.AddListener(onClick);

            var text = CreateText("Label", rect, label, 25f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(text.rectTransform, 8f);
            return button;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private static void SetCenter(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetTopBar(RectTransform rect, float height, float offset)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -offset);
            rect.sizeDelta = new Vector2(0f, height);
        }

        private static void SetTopLeft(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetTopRight(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetTopCenter(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetBottomCenter(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static Sprite CreateSolidSprite()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.DontSave,
            };
            texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateMoustacheSprite()
        {
            const int width = 160;
            const int height = 54;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.DontSave,
                filterMode = FilterMode.Bilinear,
            };

            var transparent = new Color(0f, 0f, 0f, 0f);
            var ink = new Color(0.16f, 0.06f, 0.03f, 0.98f);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var normalizedX = (x - (width * 0.5f)) / (width * 0.5f);
                    var normalizedY = (y - (height * 0.47f)) / (height * 0.47f);
                    var leftLobe = Mathf.Pow((normalizedX + 0.38f) / 0.60f, 2f) + Mathf.Pow((normalizedY + 0.10f) / 0.64f, 2f) < 1f;
                    var rightLobe = Mathf.Pow((normalizedX - 0.38f) / 0.60f, 2f) + Mathf.Pow((normalizedY + 0.10f) / 0.64f, 2f) < 1f;
                    var bridge = Mathf.Abs(normalizedX) < 0.23f && normalizedY < 0.20f && normalizedY > -0.36f;
                    texture.SetPixel(x, y, leftLobe || rightLobe || bridge ? ink : transparent);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f));
        }

        private static void DestroyRuntimeSprite(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            Destroy(sprite.texture);
            Destroy(sprite);
        }
    }
}
