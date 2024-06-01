using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMyVolunteer.Migrations {
  /// <inheritdoc />
  public partial class AddSkills: Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
          name: "Skills",
          columns: table => new {
            ID = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("PK_Skills", x => x.ID);
          });

      migrationBuilder.CreateTable(
          name: "VolunteerSkills",
          columns: table => new {
            VolunteerId = table.Column<int>(type: "int", nullable: false),
            SkillId = table.Column<int>(type: "int", nullable: false),
            Rank = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table => {
            table.PrimaryKey("PK_VolunteerSkills", x => new { x.VolunteerId, x.SkillId });
          });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(
          name: "Skills");

      migrationBuilder.DropTable(
          name: "VolunteerSkills");
    }
  }
}
