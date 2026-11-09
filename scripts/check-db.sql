-- Список таблиц в схеме public
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;

-- Структура таблицы Players
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_schema = 'public' AND table_name = 'Players'
ORDER BY ordinal_position;

-- Данные игроков
SELECT "Id", "Nickname", "Game", "Rating", "Wins", "Losses", "CreatedAt", "TeamName", "Description"
FROM "Players"
ORDER BY "Id";

-- История применённых миграций
SELECT "MigrationId", "ProductVersion"
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";

-- Сопоставление с LINQ-фильтром по рейтингу
SELECT "Nickname", "Rating"
FROM "Players"
WHERE "Rating" > 100
ORDER BY "Rating" DESC;
