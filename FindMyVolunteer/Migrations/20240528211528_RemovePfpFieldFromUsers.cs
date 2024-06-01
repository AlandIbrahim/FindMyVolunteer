using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMyVolunteer.Migrations
{
    /// <inheritdoc />
    public partial class RemovePfpFieldFromUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfilePicture",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }
    }
}
