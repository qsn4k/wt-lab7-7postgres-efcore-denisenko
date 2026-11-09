using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace wt_lab7_7postgres_efcore_denisenko.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Players",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Nickname = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                Game = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                Rating = table.Column<int>(type: "integer", nullable: false),
                Wins = table.Column<int>(type: "integer", nullable: false),
                Losses = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Players", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Players");
    }
}
