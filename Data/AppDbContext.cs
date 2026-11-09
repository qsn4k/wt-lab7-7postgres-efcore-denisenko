using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_denisenko.Models;

namespace wt_lab7_7postgres_efcore_denisenko.Data;

/// <summary>Контекст EF Core - единая точка доступа к сущностям и PostgreSQL.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var player = modelBuilder.Entity<Player>();
        player.ToTable("Players");
        player.HasKey(item => item.Id);
        player.Property(item => item.Id).ValueGeneratedOnAdd();
        player.Property(item => item.Nickname).HasMaxLength(30).IsRequired();
        player.Property(item => item.Game).HasMaxLength(60).IsRequired();
        player.Property(item => item.Rating).IsRequired();
        player.Property(item => item.Wins).IsRequired();
        player.Property(item => item.Losses).IsRequired();
        player.Property(item => item.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        player.Property(item => item.TeamName).HasMaxLength(60).HasDefaultValue("Без команды").IsRequired();
        player.Property(item => item.Description).HasMaxLength(500).HasDefaultValue(string.Empty).IsRequired();
        player.Ignore(item => item.WinRate);

        // Начальные записи перенесены из WT-6. HasData превращает их в операции миграции.
        player.HasData(
            new Player { Id = 1, Nickname = "CyberWolf",   Game = "Counter-Strike 2", Rating = 1500, Wins = 24, Losses = 12, CreatedAt = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), TeamName = "Без команды", Description = "" },
            new Player { Id = 2, Nickname = "ShadowFox",   Game = "Counter-Strike 2", Rating = 1750, Wins = 31, Losses = 10, CreatedAt = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc), TeamName = "Без команды", Description = "" },
            new Player { Id = 3, Nickname = "NeoMaster",   Game = "Dota 2",           Rating = 2100, Wins = 38, Losses = 9,  CreatedAt = new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc), TeamName = "Без команды", Description = "" },
            new Player { Id = 4, Nickname = "DarkKnight",  Game = "Dota 2",           Rating = 1850, Wins = 29, Losses = 15, CreatedAt = new DateTime(2026, 9, 4, 0, 0, 0, DateTimeKind.Utc), TeamName = "Без команды", Description = "" },
            new Player { Id = 5, Nickname = "PixelHunter", Game = "Valorant",         Rating = 1600, Wins = 21, Losses = 18, CreatedAt = new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc), TeamName = "Без команды", Description = "" },
            new Player { Id = 6, Nickname = "IceStorm",    Game = "Valorant",         Rating = 1900, Wins = 34, Losses = 11, CreatedAt = new DateTime(2026, 9, 6, 0, 0, 0, DateTimeKind.Utc), TeamName = "Без команды", Description = "" },
            new Player { Id = 7, Nickname = "RedDragon",   Game = "Counter-Strike 2", Rating = 1250, Wins = 15, Losses = 20, CreatedAt = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc), TeamName = "Без команды", Description = "" }
        );
    }
}
