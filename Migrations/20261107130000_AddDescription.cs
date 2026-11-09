using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wt_lab7_7postgres_efcore_denisenko.Migrations;

/// <inheritdoc />
public partial class AddDescription : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Players",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        // Явно фиксируем значение нового поля в начальных данных HasData.
        foreach (var id in new[] { 1, 2, 3, 4, 5, 6, 7 })
        {
            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "Id",
                keyValue: id,
                column: "Description",
                value: "");
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Description", table: "Players");
    }
}
