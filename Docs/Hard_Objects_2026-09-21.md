# Два сложных предмета

На «Свободу, ведущую народ» добавлена потемневшая монета на балке среди обломков слева внизу: координаты (0.155, 0.242), ширина 2.9% картины, поворот −8°. Теперь 5 предметов.

На «Плот Медузы» добавлен старый железный ключ на передней балке, слева от обвязки: (0.37, 0.105), ширина 5.4%, поворот −16°. Теперь 6 предметов. Координаты от нижнего левого угла. Мона Лиза и прежние предметы не изменены.

Изображения: Assets/Art/Prototype/Gameplay/HardCoin.png и HardKey.png. Размещение: Assets/UI/Gameplay/HiddenObjectCatalog.asset. Подсказки указывают участок картины, зона нажатия больше предмета. Оба новых предмета считаются обычными проверками за 2 энергии, входят в обязательный прогресс. Баланс лимитов пока прежний: 120 секунд на вторую картину, 9 проверок и 150 секунд на третью. На третьей теперь остаётся запас на 3 промаха.

Три картины теперь содержат 14 предметов. При прохождении без ошибок от 100 энергии, без естественного восстановления, расход 28, награды 6, остаток 78. Монеты за три первых завершения — 90. Старые завершённые картины остаются завершёнными, прежние частичные находки сохраняются; прогресс не сбрасывается. Для просмотра новых целей уже завершённой картины понадобится отдельный тестовый профиль; существующее сохранение намеренно не очищалось.

Сгенерировано встроенным ImageGen, подключается через Sprite Editor API с проверкой прозрачности. Приглушённая палитра и фактура рассчитаны на встраивание в живопись, но результат размещения требует ручного просмотра в игре. Сборки и игровые тесты агент не запускает.

## Промпты

Монета: Use case: stylized-concept. One transparent-background game sprite: a single worn dark bronze coin lying flat at an oblique angle, oval foreshortened face, faint indistinct worn relief NO readable numbers/text. Hand-painted as a tiny detail of an early 19th-century Romantic oil painting by Delacroix, earthy muted umber ochre charcoal, soft broken brush edges, matte dusty patina, diffuse dim warm light from upper left. No gloss, no rim highlight, no outline, no icon style, no sticker border, no background, no separate cast shadow. Coin fills central 70% width, landscape oval about 1.5:1. Genuine transparent alpha around coin.

Ключ: Use case: stylized-concept. One transparent-background game sprite: a single old small dark rusted iron skeleton key lying flat, bow at left, shaft extending to right slightly down, teeth at right. Hand-painted as a minor detail within a Gericault Romantic oil painting, coarse earthy umber black subdued grey-brown ochre pigments, soft broken brush edges, sea-worn matte corrosion. Lighting dim warm upper left, tiny restrained highlight only. No shiny metal, no high contrast, no outline, no icon style, no sticker border, no background or separate cast shadow. Key occupies central 80% width, horizontal 3:1 silhouette. Genuine transparent alpha around key.
