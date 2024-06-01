using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMyVolunteer.Migrations
{
    /// <inheritdoc />
    public partial class AddEventRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventRatings",
                columns: table => new
                {
                    FromID = table.Column<int>(type: "int", nullable: false),
                    ToID = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<byte>(type: "tinyint", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRatings", x => new { x.FromID, x.ToID });
                    table.ForeignKey(
                        name: "FK_EventRatings_AspNetUsers_FromID",
                        column: x => x.FromID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRatings_Events_ToID",
                        column: x => x.ToID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRatings_ToID",
                table: "EventRatings",
                column: "ToID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRatings");
        }
    }
}
