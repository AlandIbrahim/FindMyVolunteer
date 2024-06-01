using FindMyVolunteer.Models;
using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Data.DataTransfer {
  public class CreateSkillDTO {
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    public static implicit operator Skill(CreateSkillDTO skill) {
      return new Skill(0, skill.Name, skill.Description);
    }
  }
}
