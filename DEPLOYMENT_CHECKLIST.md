# WT-7 — контрольный список выполнения

## Локальная конфигурация и БД
- [ ] PostgreSQL установлен, создана база `wt_lab7`.
- [ ] В `appsettings.Development.json` задана локальная строка подключения (файл исключён из Git).
- [ ] `dotnet tool restore` и `dotnet restore` завершились без ошибок.
- [ ] `dotnet ef migrations list` показывает `InitialCreate`, `AddTeamName`, `SeedPlayers`, `AddDescription`.
- [ ] `dotnet ef database update` применил миграции.
- [ ] В PostgreSQL существуют `Players` и `__EFMigrationsHistory`.
- [ ] В `Players` есть `Id`, `Nickname`, `Game`, `Rating`, `Wins`, `Losses`, `CreatedAt`, `TeamName`, `Description`.
- [ ] В таблице находятся семь начальных записей WT-6.

## API и LINQ
- [ ] `/`, `/api/info`, `/api/players` возвращают `200`.
- [ ] `/api/players/1` возвращает игрока; `/api/players/999999` — `404`.
- [ ] Поиск `/api/players/search?search=cyber` возвращает отфильтрованные записи.
- [ ] `/api/players/top`, `/api/players/filter` и `/api/players/statistics/by-game` проверены.
- [ ] POST создаёт запись (`201`, Location); неправильные данные дают `400`.
- [ ] PUT изменяет запись (`200`); отсутствующий ID — `404`.
- [ ] DELETE удаляет запись (`204`); повторный GET даёт `404`.
- [ ] Swagger UI доступен по `/swagger`.
- [ ] После каждой записи результат подтверждён SQL-запросом.

## Миграции при старте
- [ ] После создания новой миграции локальный `dotnet ef database update` меняет схему.
- [ ] Перезапуск приложения применяет отсутствующие миграции через `Database.MigrateAsync()`.
- [ ] В приложении нигде не используется `EnsureCreated()`.

## Docker, GitHub и Render
- [ ] `docker build` собирает образ.
- [ ] В Render создан PostgreSQL и сохранён Internal Database URL.
- [ ] Web Service использует Runtime `Docker` и `DATABASE_URL` в Environment.
- [ ] Deploy завершился успешно; логи показывают запуск после миграций.
- [ ] Публичные URL `/`, `/api/info`, `/api/players`, `/swagger` проверены.
- [ ] После добавления ещё одного свойства создана миграция и отправлен новый commit.
- [ ] После первого Deploy создан отдельный коммит с `AddDescription`; повторный Deploy добавил столбец `Description` в PostgreSQL.
- [ ] `__EFMigrationsHistory` содержит четвёртую запись `AddDescription`.
- [ ] В отчёт внесены фактические GitHub URL, Render URL и скриншоты Dashboard, логов и SQL-клиента.

> До выполнения действий в аккаунтах GitHub/Render галочки внешней публикации и фактические ссылки оставляются незаполненными. Пароли и полные строки подключения в отчёт не включаются.
