using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Data.DataTransfer {
  public class RegisterUserDTO {
    [Required]
    [MinLength(2)]
    [RegularExpression(@"^[a-zA-Z0-9_]{1,}$", ErrorMessage = "Username contains an invalid character, allowed characters include letters, numbers and the underscore")]
    public string UserName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(8)]
    [PasswordPropertyText(true)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{1,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
    public string Password { get; set; } = string.Empty;
    [Phone]
    public string? PhoneNumber { get; set; }
    public virtual bool IsOrg => true;
  }
}
