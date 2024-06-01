using System.ComponentModel.DataAnnotations.Schema;

namespace FindMyVolunteer.Models {
  public class RequiredSkills {
    [ForeignKey("Event")]
    public int EventID { get; set; }
    [ForeignKey("Skill")]
    public int SkillID { get; set; }
    public Event Event { get; set; }
    public Skill Skill { get; set; }
  }
}
