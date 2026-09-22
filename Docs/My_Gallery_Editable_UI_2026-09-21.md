# Моя галерея — редактируемый экран

Профиль и коллекция объединены в «Мою галерею». Старые переходы к профилю перенаправлены на объединённый экран. Home и крестик сохранены.

## Где редактировать

- Assets/UI/MyGallery/MyGalleryScreen.prefab — фон, профиль, аватар, счётчики, кнопки музеев, заголовок, область прокрутки.
- Assets/UI/MyGallery/PaintingCard.prefab — отдельная рамка, подложка, полотно, название, автор, состояние, кнопка и закрытое состояние. Изменения шаблона распространяются на карточки.
- Assets/UI/MyGallery/GalleryCatalog.asset — список картин: ID, название, автор, музей, изображение и индекс игрового уровня. Для нового полотна без игрового уровня указывать chapterIndex = -1. Не дублировать индексы существующих уровней.
- Assets/Art/Prototype/MyGallery — GalleryFrame.png, GalleryPanel.png, GalleryAvatar.png, созданные встроенным ImageGen по утверждённому скетчу. Рамка и плашка настроены на растяжение с сохранением углов; внешняя прозрачность сохранена. Фон переиспользует HomeMenuBackdrop_v2.

## Прокрутка

PaintingScrollView → Viewport → PaintingGrid. Профиль и MuseumFilters находятся вне ScrollView и не двигаются вместе с картинами. Viewport обрезает изображения и нажатия за границей. Горизонтальная прокрутка выключена. Сетка всегда содержит три колонки. Grid Layout Group задаёт интервалы и отступы; AirtistGalleryGrid — отношение высоты карточки к ширине. Content Size Fitter автоматически увеличивает высоту содержимого при новых рядах. Колесо мыши и перетаскивание обрабатываются стандартным ScrollRect.

Для добавления картины достаточно новой записи каталога; экран создаёт недостающие экземпляры карточек. Не добавляйте вручную дубликаты в PaintingGrid. Сейчас каталог содержит три реальные картины Лувра; дополнительные игровые уровни и музейные коллекции ещё нужно подготовить. Полная виртуализация сотен карточек пока не реализована.

## Поведение

Счётчики читают текущий локальный прогресс. Собранная картина открывает подробности; доступная несобранная — текущий уровень. Закрытая карточка недоступна. «Все музеи» и «Лувр» фильтруют каталог; остальные музейные кнопки показывают существующие информационные карточки закрытых музеев.

Тексты — отдельные TMP-надписи, не часть картинок. Полное подключение новых подписей к Excel/таблицам локализации остаётся отдельной задачей. В Inspector можно менять статические подписи профиля; подписи полотен задаёт каталог, счётчики обновляет прогресс.

## Проверка

Компиляция успешна. Ссылки на сцену, каталог, три изображения и кнопки проверены в Edit mode. Play, игровые тесты и сборки не запускались. Визуальную проверку и проверку жестов на устройствах выполняет пользователь. Не заявляется пиксельное совпадение со скетчем без этой проверки.

# Промпты — встроенный ImageGen

+## GalleryFrame

Use case: stylized-concept. Production Unity 2D UI asset for AIrtist. Reference is style and object guide. One isolated portrait rectangular ornate polished golden picture frame matching the three frames in the reference exactly in feel, rounded ornate gold corners, thin carved gold borders. Portrait aspect 4:5. Completely empty genuine transparent alpha center and transparent exterior. No painting, no plaque, no text, no shadow beyond tight frame edge. Frame fills canvas with only 2 percent transparent margin. Straight on orthographic UI sprite. High polish, crisp painterly edges, luminous honey gold and warm cream palette from reference. Output ONLY the requested single asset, not a full mockup. No watermark.

## GalleryPanel

Use case: stylized-concept. Production Unity 2D UI asset for AIrtist. Reference is style and object guide. One isolated wide horizontal cream parchment UI panel with fine gold double border, rounded corners and small delicate gold corner flourishes. Match the profile band in reference. Blank uninterrupted creamy center, no avatar, no dividers, NO words or icons. Aspect ratio 4:1. Panel fills canvas with 2 percent margin. Transparent alpha outside rounded corners, NOT transparent inside. Straight on game UI sprite usable for nine-slicing. No brown wood. High polish, crisp painterly edges, luminous honey gold and warm cream palette from reference. Output ONLY the requested single asset, not a full mockup. No watermark.

## GalleryAvatar

Use case: stylized-concept. Production Unity 2D UI asset for AIrtist. Reference is style and object guide. One isolated circular gold-rimmed profile avatar of Amelie exactly matching the reference character face: expressive young adult brunette artist with brown eyes, messy bun and red hair ribbon, striped cream and black shirt, warm friendly smile, head and shoulders filling the circular crop. Casual dimensional painterly mobile game art. Square canvas with circle filling 92 percent. Warm cream inside the circular portrait, transparent alpha outside. No text or badges. High polish, crisp painterly edges, luminous honey gold and warm cream palette from reference. Output ONLY the requested single asset, not a full mockup. No watermark.
