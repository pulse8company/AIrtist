# AIrtist — музеи и мини-иконки

## Обновление: единая панель и положение музеев

Предыдущая раскладка с выносными линиями заменена по просьбе пользователя. Значки теперь 42×42 вместо 100×100, без точек, линий и автоматического разнесения. Они привязаны непосредственно к городам и увеличиваются вместе с картой. Нью-Йорк перенесён на побережье; пиксель привязки проверен по исходному изображению. Карта художественная, поэтому координаты сопоставлены с её контурами, не с географической проекцией.

Все экраны используют одну верхнюю панель из элементов Home: логотип, Париж · первая выставка, подарок, карта, коллекция, магазин, профиль. Справа крестик всегда открывает Home; на Home скрыт. Старые панели других экранов не создаются при наличии утверждённого Home. Нижние кнопки масштаба используют кремово-золотую поверхность кнопки Home. Существующие обработчики увеличения/уменьшения сохранены.

Компиляция проверена. Визуальная проверка в Play и на устройствах остаётся пользователю; запусков и сборок не выполнялось. Ниже сохранено описание предыдущей версии и исходные промпты.

Созданы 4 иллюстрации музеев, 4 отдельные мини-иконки с прозрачностью и деревянно-золотая рамка. Изображения созданы встроенным ImageGen. Папка: Assets/Art/Prototype/Museums; иконки: MapIcons.

Лувр доступен. Метрополитен (Нью-Йорк), Эрмитаж и Прадо показывают отдельные мини-замочки; карточки доступны для просмотра, вход закрыт программно.

Маршрут: карта → карточка музея с фактом и прогрессом → Лувр → картины. Существующие сохранение находок, коллекция, следующая картина и Home сохранены. Музейные экраны получили рамки и шрифт в стиле Home.

Мини-иконки имеют постоянный размер относительно карты при зуме. Области нажатия увеличены. Автоматическая раскладка разносит пересекающиеся значки и соединяет их с точками городов. Координаты вручную сопоставлены с иллюстрированной картой, это не географическая проекция. При чрезмерной плотности недоступный для размещения значок скрывается до изменения масштаба/позиции; полноценная кластеризация — следующий этап при расширении каталога.

Компиляция успешна. Play, игровые тесты и сборки не запускались. Требуется пользовательская проверка на телефоне, планшете и большом экране. Новые музейные тексты пока русские, перенос в таблицу локализации остаётся отдельным этапом.

Источники фактов: https://www.louvre.fr/en/explore/the-palace ; https://www.metmuseum.org/de/about-the-met/history ; https://www.hermitagemuseum.org/uploads/files/otchet_2015_en.pdf ; https://www.museodelprado.es/en/museum/history-of-the-museum

# Промпты генерации

+## Louvre

Use case: stylized-concept. Production illustration for AIrtist 2D landscape mobile museum adventure. Make ONE beautiful rich saturated painterly museum postcard image, wide 3:2 composition. Subject: Louvre Museum in Paris, recognizable glass pyramid in the Cour Napoleon courtyard with magnificent honey limestone Louvre palace behind, turquoise reflections in shallow fountain, roses and lush greenery framing lower corners. Warm sunny golden-hour light, jewel teal shadows, rich leaf greens, soft coral flowers, creamy stone and polished golden highlights, dimensional friendly rounded forms, exquisite casual game illustration comparable to Gardenscapes/Homescapes, consistent with supplied Home reference in palette and painterly finish. Architecture fills central 75 percent, identifiable silhouettes, eye-level slightly elevated three-quarter view, simplified clean large shapes readable in thumbnails, no crowds. Full bleed scenery, NO UI, NO frame, NO lettering, NO logos, NO watermark, NO padlocks (added as separate Unity UI). Reference image is STYLE ONLY; no woman or Home interface.

## Met

Use case: stylized-concept. Production illustration for AIrtist 2D landscape mobile museum adventure. Make ONE beautiful rich saturated painterly museum postcard image, wide 3:2 composition. Subject: Metropolitan Museum of Art Fifth Avenue New York, recognizable grand Beaux-Arts limestone facade, three monumental arched entrances, broad entrance steps, fountains and lush Central Park trees. Warm sunny golden-hour light, jewel teal shadows, rich leaf greens, soft coral flowers, creamy stone and polished golden highlights, dimensional friendly rounded forms, exquisite casual game illustration comparable to Gardenscapes/Homescapes, consistent with supplied Home reference in palette and painterly finish. Architecture fills central 75 percent, identifiable silhouettes, eye-level slightly elevated three-quarter view, simplified clean large shapes readable in thumbnails, no crowds. Full bleed scenery, NO UI, NO frame, NO lettering, NO logos, NO watermark, NO padlocks (added as separate Unity UI). Reference image is STYLE ONLY; no woman or Home interface.

## Hermitage

Use case: stylized-concept. Production illustration for AIrtist 2D landscape mobile museum adventure. Make ONE beautiful rich saturated painterly museum postcard image, wide 3:2 composition. Subject: State Hermitage Winter Palace in Saint Petersburg, recognizable mint emerald and white baroque palace facade with gold details, elegant rows of windows, broad Palace Square, glowing pink sunset. Warm sunny golden-hour light, jewel teal shadows, rich leaf greens, soft coral flowers, creamy stone and polished golden highlights, dimensional friendly rounded forms, exquisite casual game illustration comparable to Gardenscapes/Homescapes, consistent with supplied Home reference in palette and painterly finish. Architecture fills central 75 percent, identifiable silhouettes, eye-level slightly elevated three-quarter view, simplified clean large shapes readable in thumbnails, no crowds. Full bleed scenery, NO UI, NO frame, NO lettering, NO logos, NO watermark, NO padlocks (added as separate Unity UI). Reference image is STYLE ONLY; no woman or Home interface.

## Prado

Use case: stylized-concept. Production illustration for AIrtist 2D landscape mobile museum adventure. Make ONE beautiful rich saturated painterly museum postcard image, wide 3:2 composition. Subject: Museo Nacional del Prado in Madrid, recognizable Villanueva neoclassical facade, six-column Doric entrance portico, warm pink brick and cream stone, statue of Velazquez in front, leafy Spanish gardens. Warm sunny golden-hour light, jewel teal shadows, rich leaf greens, soft coral flowers, creamy stone and polished golden highlights, dimensional friendly rounded forms, exquisite casual game illustration comparable to Gardenscapes/Homescapes, consistent with supplied Home reference in palette and painterly finish. Architecture fills central 75 percent, identifiable silhouettes, eye-level slightly elevated three-quarter view, simplified clean large shapes readable in thumbnails, no crowds. Full bleed scenery, NO UI, NO frame, NO lettering, NO logos, NO watermark, NO padlocks (added as separate Unity UI). Reference image is STYLE ONLY; no woman or Home interface.

## Panel

Use case: stylized-concept. One production 2D UI panel texture for AIrtist casual museum adventure, landscape 3:2 rectangle, straight-on orthographic. Rich honey walnut wooden outer frame, fine bevelled gold filigree ONLY within outer 8 percent edges and four corners, ivory cream parchment center EMPTY and uniform with very subtle paper texture. Rounded outer corners. Jewel highlights, polished dimensional painterly casual game style matching attached reference. Panel fills entire canvas edge to edge, no surrounding scene, NO words, NO symbols, NO buttons, NO museum, NO characters. Central 80 percent MUST remain empty light cream to hold native text. Suitable for nine-slice scaling with straight repeatable edge runs and small ornamental corners. Reference is style only.

## LouvreIcon

Use case: stylized-concept. ONE isolated miniature museum building icon: Louvre palace pavilion behind its iconic glass pyramid. For AIrtist world map, must read beautifully at 80 pixels. Compact near-square three-quarter isometric landmark silhouette, simple chunky volumes, very few tiny details, vibrant honey gold, teal, cream and coral palette, soft dimensional painterly casual mobile game style. Building occupies central 85 percent, no rectangular image border, NO sky NO landscape NO flowers NO people NO words NO logos NO padlock. Genuine transparent alpha background, not checkerboard or solid white. Subtle tight contact shadow. This is a small map icon, not a postcard or a photographic scene.

## MetIcon

Use case: stylized-concept. ONE isolated miniature museum building icon: Metropolitan Museum of Art Fifth Avenue, recognizable three arched entrances and broad stairs. For AIrtist world map, must read beautifully at 80 pixels. Compact near-square three-quarter isometric landmark silhouette, simple chunky volumes, very few tiny details, vibrant honey gold, teal, cream and coral palette, soft dimensional painterly casual mobile game style. Building occupies central 85 percent, no rectangular image border, NO sky NO landscape NO flowers NO people NO words NO logos NO padlock. Genuine transparent alpha background, not checkerboard or solid white. Subtle tight contact shadow. This is a small map icon, not a postcard or a photographic scene.

## HermitageIcon

Use case: stylized-concept. ONE isolated miniature museum building icon: Winter Palace Hermitage, mint emerald and white baroque palace with gold trim. For AIrtist world map, must read beautifully at 80 pixels. Compact near-square three-quarter isometric landmark silhouette, simple chunky volumes, very few tiny details, vibrant honey gold, teal, cream and coral palette, soft dimensional painterly casual mobile game style. Building occupies central 85 percent, no rectangular image border, NO sky NO landscape NO flowers NO people NO words NO logos NO padlock. Genuine transparent alpha background, not checkerboard or solid white. Subtle tight contact shadow. This is a small map icon, not a postcard or a photographic scene.

## PradoIcon

Use case: stylized-concept. ONE isolated miniature museum building icon: Prado Villanueva building, six-column portico, warm pink brick, tiny seated Velazquez statue. For AIrtist world map, must read beautifully at 80 pixels. Compact near-square three-quarter isometric landmark silhouette, simple chunky volumes, very few tiny details, vibrant honey gold, teal, cream and coral palette, soft dimensional painterly casual mobile game style. Building occupies central 85 percent, no rectangular image border, NO sky NO landscape NO flowers NO people NO words NO logos NO padlock. Genuine transparent alpha background, not checkerboard or solid white. Subtle tight contact shadow. This is a small map icon, not a postcard or a photographic scene.
