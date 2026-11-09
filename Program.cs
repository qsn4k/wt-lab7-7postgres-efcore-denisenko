using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Npgsql;
using wt_lab7_7postgres_efcore_denisenko.Data;
using wt_lab7_7postgres_efcore_denisenko.Models;

var builder = WebApplication.CreateBuilder(args);

// DATABASE_URL имеет приоритет на Render. Локально используется appsettings.Development.json.
var connectionString = BuildConnectionString(builder.Configuration);
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Не задано подключение к PostgreSQL. Настройте ConnectionStrings:DefaultConnection или DATABASE_URL.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tournament Hub REST API",
        Version = "v1",
        Description = "REST API для управления профилями игроков киберспортивного сервиса. Данные хранятся в PostgreSQL через Entity Framework Core."
    });
});

var app = builder.Build();

// При каждом запуске применяются только ещё не применённые миграции.
// EnsureCreated() намеренно не используется: проект управляет схемой через миграции.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tournament Hub API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Tournament Hub API - Swagger";
});

app.MapGet("/", () => Results.Text("Tournament Hub API работает!", "text/plain; charset=utf-8"))
    .WithName("Root")
    .WithTags("Service")
    .WithSummary("Проверка доступности приложения")
    .Produces<string>(StatusCodes.Status200OK, "text/plain");

app.MapGet("/api/info", () => Results.Ok(new
    {
        application = "Tournament Hub",
        technology = "ASP.NET Core Minimal API",
        orm = "Entity Framework Core",
        database = "PostgreSQL",
        version = "2.0.0",
        description = "REST API для управления профилями игроков",
        documentation = "/swagger"
    }))
    .WithName("GetApiInfo")
    .WithTags("Service")
    .WithSummary("Получение сведений о приложении");

// Полный список. Фильтры и сортировка выполняются на стороне PostgreSQL.
app.MapGet("/api/players", async (
    string? search,
    string? game,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    IQueryable<Player> query = db.Players.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim();
        query = query.Where(player =>
            EF.Functions.ILike(player.Nickname, $"%{term}%") ||
            EF.Functions.ILike(player.Game, $"%{term}%"));
    }

    if (!string.IsNullOrWhiteSpace(game))
    {
        var selectedGame = game.Trim();
        query = query.Where(player => player.Game == selectedGame);
    }

    var players = await query
        .OrderByDescending(player => player.Rating)
        .ThenBy(player => player.Nickname)
        .ToListAsync(cancellationToken);

    return Results.Ok(players);
})
    .WithName("GetPlayers")
    .WithTags("Players")
    .WithSummary("Получить список игроков")
    .WithDescription("Поддерживает поиск по никнейму/игре и фильтрацию по названию игры.")
    .Produces<List<Player>>(StatusCodes.Status200OK);

app.MapGet("/api/players/{id:int}", async (
    int id,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var player = await db.Players.AsNoTracking()
        .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    return player is null ? Results.NotFound() : Results.Ok(player);
})
    .WithName("GetPlayerById")
    .WithTags("Players")
    .WithSummary("Получить профиль игрока по ID")
    .Produces<Player>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// Отдельный адрес поиска оставлен для выполнения пункта ЛР-7 и удобства демонстрации LINQ.
app.MapGet("/api/players/search", async (
    string? search,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    IQueryable<Player> query = db.Players.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim();
        query = query.Where(player =>
            EF.Functions.ILike(player.Nickname, $"%{term}%") ||
            EF.Functions.ILike(player.Game, $"%{term}%"));
    }

    return Results.Ok(await query
        .OrderBy(player => player.Nickname)
        .ToListAsync(cancellationToken));
})
    .WithName("SearchPlayers")
    .WithTags("LINQ")
    .WithSummary("Поиск игроков по никнейму или игре")
    .Produces<List<Player>>(StatusCodes.Status200OK);

app.MapGet("/api/players/top", async (
    int? count,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var take = Math.Clamp(count ?? 5, 1, 50);
    var players = await db.Players.AsNoTracking()
        .OrderByDescending(player => player.Rating)
        .ThenByDescending(player => player.Wins)
        .ThenBy(player => player.Nickname)
        .Take(take)
        .ToListAsync(cancellationToken);

    return Results.Ok(players);
})
    .WithName("GetTopPlayers")
    .WithTags("LINQ")
    .WithSummary("Получить рейтинг лучших игроков")
    .Produces<List<Player>>(StatusCodes.Status200OK);

app.MapGet("/api/players/filter", async (
    int? minRating,
    int? minWins,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    IQueryable<Player> query = db.Players.AsNoTracking();
    if (minRating.HasValue)
        query = query.Where(player => player.Rating >= minRating.Value);
    if (minWins.HasValue)
        query = query.Where(player => player.Wins >= minWins.Value);

    var players = await query
        .OrderByDescending(player => player.Rating)
        .ToListAsync(cancellationToken);

    return Results.Ok(players);
})
    .WithName("FilterPlayers")
    .WithTags("LINQ")
    .WithSummary("Фильтрация игроков по минимальному рейтингу и победам")
    .Produces<List<Player>>(StatusCodes.Status200OK);

app.MapGet("/api/players/statistics/by-game", async (
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var statistics = await db.Players.AsNoTracking()
        .GroupBy(player => player.Game)
        .Select(group => new
        {
            game = group.Key,
            playersCount = group.Count(),
            averageRating = group.Average(player => player.Rating),
            maximumRating = group.Max(player => player.Rating),
            totalWins = group.Sum(player => player.Wins)
        })
        .OrderByDescending(item => item.playersCount)
        .ThenBy(item => item.game)
        .ToListAsync(cancellationToken);

    return Results.Ok(statistics);
})
    .WithName("GetPlayerStatisticsByGame")
    .WithTags("LINQ")
    .WithSummary("Агрегированная статистика игроков по играм")
    .Produces(StatusCodes.Status200OK);

app.MapPost("/api/players", async (
    PlayerRequest request,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var errors = ValidateRequest(request);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var player = new Player
    {
        Nickname = request.Nickname.Trim(),
        Game = request.Game.Trim(),
        TeamName = string.IsNullOrWhiteSpace(request.TeamName) ? "Без команды" : request.TeamName.Trim(),
        Description = request.Description?.Trim() ?? string.Empty,
        Rating = request.Rating,
        Wins = request.Wins,
        Losses = request.Losses,
        CreatedAt = DateTime.UtcNow
    };

    db.Players.Add(player);
    await db.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/players/{player.Id}", player);
})
    .WithName("CreatePlayer")
    .WithTags("Players")
    .WithSummary("Создать профиль игрока")
    .WithDescription("ID и дата регистрации назначаются сервером.")
    .Accepts<PlayerRequest>("application/json")
    .Produces<Player>(StatusCodes.Status201Created)
    .ProducesValidationProblem();

app.MapPut("/api/players/{id:int}", async (
    int id,
    PlayerRequest request,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var player = await db.Players.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    if (player is null)
        return Results.NotFound();

    var errors = ValidateRequest(request);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    player.Nickname = request.Nickname.Trim();
    player.Game = request.Game.Trim();
    player.TeamName = string.IsNullOrWhiteSpace(request.TeamName) ? "Без команды" : request.TeamName.Trim();
    player.Description = request.Description?.Trim() ?? string.Empty;
    player.Rating = request.Rating;
    player.Wins = request.Wins;
    player.Losses = request.Losses;

    await db.SaveChangesAsync(cancellationToken);
    return Results.Ok(player);
})
    .WithName("UpdatePlayer")
    .WithTags("Players")
    .WithSummary("Изменить профиль игрока")
    .Accepts<PlayerRequest>("application/json")
    .Produces<Player>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesValidationProblem();

app.MapDelete("/api/players/{id:int}", async (
    int id,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var player = await db.Players.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    if (player is null)
        return Results.NotFound();

    db.Players.Remove(player);
    await db.SaveChangesAsync(cancellationToken);
    return Results.NoContent();
})
    .WithName("DeletePlayer")
    .WithTags("Players")
    .WithSummary("Удалить профиль игрока")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

static Dictionary<string, string[]> ValidateRequest(PlayerRequest request)
{
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(request);
    Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true);

    return validationResults
        .SelectMany(result => result.MemberNames.DefaultIfEmpty("request")
            .Select(member => new { Member = member, Message = result.ErrorMessage ?? "Некорректное значение." }))
        .GroupBy(item => item.Member)
        .ToDictionary(group => group.Key, group => group.Select(item => item.Message).Distinct().ToArray());
}

static string? BuildConnectionString(IConfiguration configuration)
{
    var databaseUrl = configuration["DATABASE_URL"];
    if (string.IsNullOrWhiteSpace(databaseUrl))
        return configuration.GetConnectionString("DefaultConnection");

    if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
        throw new InvalidOperationException("DATABASE_URL должен содержать корректный PostgreSQL URI.");

    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(uri.AbsolutePath.Trim('/')))
        throw new InvalidOperationException("DATABASE_URL должен содержать имя пользователя, пароль и имя базы данных.");

    var requireTls = uri.Query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(part => part.Split('=', 2))
        .Any(pair => pair.Length == 2 &&
            Uri.UnescapeDataString(pair[0]).Equals("sslmode", StringComparison.OrdinalIgnoreCase) &&
            Uri.UnescapeDataString(pair[1]).Equals("require", StringComparison.OrdinalIgnoreCase));

    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/')),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = requireTls ? SslMode.Require : SslMode.Disable
    };

    return csb.ConnectionString;
}

var port = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrWhiteSpace(port))
    port = "10000";

app.Run($"http://0.0.0.0:{port}");
