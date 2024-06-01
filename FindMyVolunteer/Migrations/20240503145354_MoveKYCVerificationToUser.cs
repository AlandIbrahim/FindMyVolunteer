using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMyVolunteer.Migrations
{
    /// <inheritdoc />
    public partial class MoveKYCVerificationToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KYCVerified",
                table: "Volunteers");

            migrationBuilder.AddColumn<bool>(
                name: "KYCVerified",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KYCVerified",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "KYCVerified",
                table: "Volunteers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
