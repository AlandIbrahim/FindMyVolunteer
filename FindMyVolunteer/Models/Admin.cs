using FindMyVolunteer.Models.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace FindMyVolunteer.Models {
  public class Admin {
    [ForeignKey("AppUser")]
    public int ID { get; set; }
    public AppUser User { get; set; }
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    public int UsersValidated { get; set; } = 0; // number of users validated by this admin
  }
}
