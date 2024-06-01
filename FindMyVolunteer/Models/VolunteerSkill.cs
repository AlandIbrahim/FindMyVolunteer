using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Models {
  public class VolunteerSkill {
    public VolunteerSkill() { }
    public VolunteerSkill(int volunteerId, int skillId) {
      VolunteerId = volunteerId;
      SkillId = skillId;
    }
    public VolunteerSkill(int volunteerId, int skillId, int rank) {
      VolunteerId = volunteerId;
      SkillId = skillId;
      Rank = rank;
    }
    public int VolunteerId { get; set; }
    public int SkillId { get; set; }
    public int Rank { get; set; } // starts from 1, 1 is the highest rank
  }
}
