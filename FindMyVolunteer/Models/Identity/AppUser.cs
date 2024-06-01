using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Models.Identity {
  public class AppUser: IdentityUser<int> {
    public bool IsOrganization { get; set; }
    public bool KYCVerified { get; set; }
    public override string? UserName { get => base.UserName; set => base.UserName = value; }
    public AppUser() { }
    public AppUser(string userName, string email, string phoneNumber, bool isOrg) {
      UserName = userName;
      Email = email;
      PhoneNumber = phoneNumber;
      IsOrganization = isOrg;
    }
  }
}
