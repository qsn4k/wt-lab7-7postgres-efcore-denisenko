using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wt_lab7_7postgres_efcore_denisenko.Migrations;

/// <inheritdoc />
public partial class SeedPlayers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Players",
            columns: new[] { "Id", "CreatedAt", "Game", "Losses", "Nickname", "Rating", "TeamName", "Wins" },
            values: new object[,]
            {
                { 1, new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), "Counter-Strike 2", 12, "CyberWolf", 1500, "Без команды", 24 },
                { 2, new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc), "Counter-Strike 2", 10, "ShadowFox", 1750, "Без команды", 31 },
                { 3, new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc), "Dota 2", 9, "NeoMaster", 2100, "Без команды", 38 },
                { 4, new DateTime(2026, 9, 4, 0, 0, 0, DateTimeKind.Utc), "Dota 2", 15, "DarkKnight", 1850, "Без команды", 29 },
                { 5, new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc), "Valorant", 18, "PixelHunter", 1600, "Без команды", 21 },
                { 6, new DateTime(2026, 9, 6, 0, 0, 0, DateTimeKind.Utc), "Valorant", 11, "IceStorm", 1900, "Без команды", 34 },
                { 7, new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc), "Counter-Strike 2", 20, "RedDragon", 1250, "Без команды", 15 }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "Players",
            keyColumn: "Id",
            keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7 });
    }
}
