using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMyVolunteer.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAgeWithBirthday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Volunteers");

            migrationBuilder.RenameColumn(
                name: "FamiliarLanguages",
                table: "Volunteers",
                newName: "Languages");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Birthday",
                table: "Volunteers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "Volunteers");

            migrationBuilder.RenameColumn(
                name: "Languages",
                table: "Volunteers",
                newName: "FamiliarLanguages");

            migrationBuilder.AddColumn<byte>(
                name: "Age",
                table: "Volunteers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
