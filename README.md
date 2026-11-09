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

# wt-lab7-7postgres-efcore-denisenko
