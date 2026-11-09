# WT-7 — PostgreSQL и Entity Framework Core (Tournament Hub)

Лабораторная работа № 7 по дисциплине «Web-технологии». Предметная область Tournament Hub и REST API профилей игроков, а временная коллекция `PlayerStore` заменена PostgreSQL через Entity Framework Core.

## Состав и технологии

- .NET 10 / ASP.NET Core Minimal API;
- Entity Framework Core 10 и провайдер Npgsql 10;
- PostgreSQL;
- подход Code First и четыре миграции (`InitialCreate`, `AddTeamName`, `SeedPlayers`, `AddDescription`);
- CRUD и LINQ-запросы к PostgreSQL;
- Swagger/OpenAPI;
- Docker multi-stage build и конфигурация для Render.



# wt-lab7-7postgres-efcore-denisenko
