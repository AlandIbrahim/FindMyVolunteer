using FindMyVolunteer.Data.Types;
using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Data.DataTransfer {
  public class RegisterVolunteerDTO: RegisterUserDTO {
    [Required]
    [MinLength(2)]
    [MaxLength(16)]
    [RegularExpression(@"^[a-zA-Z]{1,}$", ErrorMessage = "First name contains an invalid character, only letters are allowed")]
    public string FirstName { get; set; } = string.Empty;
    [MinLength(2)]
    [MaxLength(16)]
    [RegularExpression(@"^[a-zA-Z]{1,}$", ErrorMessage = "Middle name contains an invalid character, only letters are allowed")]
    public string? MiddleName { get; set; }
    [Required]
    [MinLength(2)]
    [MaxLength(16)]
    [RegularExpression(@"^[a-zA-Z]{1,}$", ErrorMessage = "Last name contains an invalid character, only letters are allowed")]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Birthday { get; set; }
    [Required]
    public bool Gender { get; set; }

    [Required]
    public City City { get; set; }
    [Required]
    public string Languages { get; set; } = "Kurdish";
    public override bool IsOrg => false;
  }
}
