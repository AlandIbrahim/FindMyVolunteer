using FindMyVolunteer.Models;
using FindMyVolunteer.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Data.DataTransfer {
  public class RegisterOrganizationDTO: RegisterUserDTO {
    [Required]
    [MinLength(2)]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MinLength(2)]
    [MaxLength(64)]
    public string Address { get; set; } = string.Empty;
    [MinLength(2)]
    [MaxLength(64)]
    public string? BusinessLicense { get; set; }
    [Required]
    public bool IsGovernmental { get; set; }
    public static implicit operator Organization(RegisterOrganizationDTO org) => new() {
      Name = org.Name,
      Address = org.Address,
      BusinessLicense = org.BusinessLicense,
      IsGovernmental = org.IsGovernmental
    };
    public static implicit operator AppUser(RegisterOrganizationDTO org) => new() {
      UserName = org.UserName,
      Email = org.Email,
      PhoneNumber = org.PhoneNumber,
      IsOrganization = true
    };
  }
}
