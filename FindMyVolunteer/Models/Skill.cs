using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Models {
  public class Skill {
    public Skill() { }
    public Skill(int id, string name, string description) {
      ID = id;
      Name = name;
      Description = description;
    }
    [Key]
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } = "";

  }
}
