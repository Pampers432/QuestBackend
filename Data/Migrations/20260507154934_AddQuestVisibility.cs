using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // В БД уже существуют все таблицы (они создавались через EnsureCreated()).
            // Поэтому миграция должна НЕ создавать таблицы заново, а лишь добавить колонку Visibility в Quests.
            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "Quests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Quests");
        }
    }
}
