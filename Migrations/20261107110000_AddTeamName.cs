using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wt_lab7_7postgres_efcore_denisenko.Migrations;

/// <inheritdoc />
public partial class AddTeamName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TeamName",
            table: "Players",
            type: "character varying(60)",
            maxLength: 60,
            nullable: false,
            defaultValue: "Без команды");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "TeamName", table: "Players");
    }
}
