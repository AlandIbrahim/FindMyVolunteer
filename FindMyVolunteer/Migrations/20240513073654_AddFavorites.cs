using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMyVolunteer.Migrations {
  /// <inheritdoc />
  public partial class AddFavorites: Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
          name: "Favorites",
          columns: table => new {
            UserId = table.Column<int>(type: "int", nullable: false),
            EventId = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("PK_Favorites", x => new { x.UserId, x.EventId });
          });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(
          name: "Favorites");
    }
  }
}
