# WT-7 — PostgreSQL и Entity Framework Core (Tournament Hub)

Лабораторная работа № 7 по дисциплине «Web-технологии». Проект является развитием исходного WT-6 `wt-lab6-6minimalapi-denisenko`: сохранена предметная область Tournament Hub и REST API профилей игроков, а временная коллекция `PlayerStore` заменена PostgreSQL через Entity Framework Core.

## Состав и технологии

- .NET 10 / ASP.NET Core Minimal API;
- Entity Framework Core 10 и провайдер Npgsql 10;
- PostgreSQL;
- подход Code First и четыре миграции (`InitialCreate`, `AddTeamName`, `SeedPlayers`, `AddDescription`);
- CRUD и LINQ-запросы к PostgreSQL;
- Swagger/OpenAPI;
- Docker multi-stage build и конфигурация для Render.

`WinRate` вычисляется при формировании ответа и не сохраняется в таблице. `CreatedAt` уже входил в модель WT-6, поэтому он включён в начальную схему `InitialCreate`; следующая миграция добавляет новое свойство `TeamName`, а после первой публикации отдельная миграция `AddDescription` расширяет схему ещё одним полем. Это демонстрирует полный цикл повторного Deploy.

## Запуск локально

1. Установите .NET 10 SDK и PostgreSQL.
2. Создайте базу `wt_lab7` и пользователя PostgreSQL.
3. Скопируйте `appsettings.Development.example.json` в `appsettings.Development.json`, укажите свой локальный пароль. Этот файл исключён из Git.
4. В каталоге проекта выполните:

```bash
dotnet tool restore
dotnet restore
dotnet ef migrations list
dotnet ef database update
dotnet run
```

При запуске приложение само вызывает `Database.MigrateAsync()`. Команда `database update` выше нужна для отдельной демонстрации применения миграций из CLI; повторный запуск безопасен.

Откройте `http://localhost:10000/swagger`.

## Строка подключения

Локально читается `ConnectionStrings:DefaultConnection`. Для Render задаётся `DATABASE_URL` в Environment Web Service. Секреты не добавляются в `appsettings.json`, Git, README, HTTP-примеры или отчёт. В репозитории оставлен только демонстрационный пароль `CHANGE_ME`.

В URI-подключении параметр `sslmode=require` включает TLS. Для внутреннего URL Render без `sslmode=require` используется отключённый TLS в соответствии с внутренней схемой подключения задания. В Render рекомендуется Internal Database URL, когда Web Service и PostgreSQL находятся в одном регионе и аккаунте.

## API

| Метод | Endpoint | Назначение |
|---|---|---|
| GET | `/` | Проверка доступности |
| GET | `/api/info` | Информация о приложении |
| GET | `/api/players` | Список, `search` и `game` |
| GET | `/api/players/{id}` | Игрок по ID |
| GET | `/api/players/search?search=cyber` | Поиск по никнейму/игре |
| GET | `/api/players/top?count=5` | Рейтинг лучших игроков |
| GET | `/api/players/filter?minRating=1500&minWins=20` | Фильтрация по рейтингу и победам |
| GET | `/api/players/statistics/by-game` | Группировка и агрегаты по играм |
| POST | `/api/players` | Создание профиля |
| PUT | `/api/players/{id}` | Изменение профиля |
| DELETE | `/api/players/{id}` | Удаление профиля |

Тело POST/PUT:

```json
{
  "nickname": "ApiTester",
  "game": "Valorant",
  "teamName": "Без команды",
  "description": "Профиль игрока Tournament Hub",
  "rating": 1200,
  "wins": 8,
  "losses": 3
}
```

`Id` и `CreatedAt` назначаются сервером. При отсутствии `teamName` используется `Без команды`; `description` может быть пустым.

## Миграции

- `InitialCreate` — таблица `Players` на основе модели WT-6, включая `CreatedAt`.
- `AddTeamName` — добавление обязательного столбца `TeamName` со значением по умолчанию.
- `SeedPlayers` — семь тестовых профилей WT-6, добавленные через `HasData`.
- `AddDescription` — добавление поля `Description` (до 500 символов) отдельной миграцией для повторной публикации.

Команды:

```bash
dotnet ef migrations list
dotnet ef database update
dotnet ef migrations add AddAnotherField
dotnet ef database update
```

После изменения `AppDbContext`/модели следует создать новую миграцию командой `dotnet ef migrations add <MigrationName>` и сохранить её в Git.

## SQL-проверки

Файл `scripts/check-db.sql` содержит запросы для просмотра таблиц, столбцов, записей и истории миграций. Для PostgreSQL имена таблиц и столбцов, созданные EF, заключаются в двойные кавычки.

## Docker

```bash
docker build -t wt-lab7-7postgres-efcore .
docker run --rm -p 10000:10000 \
  -e DATABASE_URL="postgresql://user:password@host:5432/database?sslmode=require" \
  wt-lab7-7postgres-efcore
```

Для локальной базы можно использовать переменную `ConnectionStrings__DefaultConnection` вместо `DATABASE_URL`.

## Публикация на Render

1. Отправьте проект в отдельный GitHub-репозиторий.
2. В Render создайте PostgreSQL и скопируйте **Internal Database URL**.
3. Создайте Web Service из GitHub-репозитория, Runtime `Docker`.
4. В `Environment` добавьте `DATABASE_URL` со значением Internal Database URL.
5. Выполните Deploy и проверьте Logs, `/`, `/api/info`, `/api/players` и `/swagger`.
6. Выполните SQL-проверки `Players` и `__EFMigrationsHistory` через SQL-клиент Render.

При каждом старте Web Service приложение применяет неприменённые миграции автоматически. Для демонстрации повторного Deploy используйте двухэтапную публикацию. Скрипт `scripts/create-git-history.ps1` создаёт три коммита базовой версии с миграциями `InitialCreate`, `AddTeamName` и `SeedPlayers`, а затем возвращает изменения `Description` в рабочую папку незакоммиченными. Сначала добавьте GitHub remote и отправьте базовые коммиты (`git push -u origin main`), дождитесь первого Deploy. Только после успешной первой публикации выполните `git add .`, `git commit -m "Add player description migration"`, `git push`. Второй Deploy применит `AddDescription` автоматически. Не добавляйте незакоммиченные изменения до первого Deploy.

## Smoke test

После локального запуска выполните в PowerShell:

```powershell
./scripts/smoke-test.ps1 -BaseUrl "http://localhost:10000"
```

Сценарий проверяет чтение, 404, LINQ endpoints, POST, PUT и DELETE. Он не заменяет проверку реальной базы SQL-клиентом.
# wt-lab7-7postgres-efcore-denisenko
