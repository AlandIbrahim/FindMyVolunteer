using FindMyVolunteer.Data.Types;
using FindMyVolunteer.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FindMyVolunteer.Models {
  public class Volunteer {
    [ForeignKey("AppUser")]
    public int ID { get; set; }
    public AppUser AppUser { get; set; } = null!;

    [Required]
    [StringLength(16)]
    public string FirstName { get; set; }

    [StringLength(16)]
    public string? MiddleName { get; set; }

    [Required]
    [StringLength(16)]
    public string LastName { get; set; }
    [Required]
    public DateOnly Birthday { get; set; }
    [Required]
    public bool Gender { get; set; } = false; //true for woman, false for man.
    #region KYC
    public bool KYCIsPassport { get; set; } = false;
    #endregion
    [Required]
    public Languages Languages { get; set; }
    [Required]
    public City City { get; set; }
    public Volunteer() { }
    public Volunteer(int uid, string firstName, string lastName, DateOnly birthday, bool gender, Languages languages, City city) {
      ID = uid;
      FirstName = firstName;
      LastName = lastName;
      Birthday = birthday;
      Gender = gender;
      Languages = languages;
      City = city;
    }
  }
}
