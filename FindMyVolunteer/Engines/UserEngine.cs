using FindMyVolunteer.Data;
using FindMyVolunteer.Data.DataTransfer;
using FindMyVolunteer.Data.Types;
using FindMyVolunteer.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;

namespace FindMyVolunteer.Engines
{
    public class UserEngine(UserManager<AppUser> uMan, SignInManager<AppUser> sMan) {
    public async Task<LoginResultDTO> LoginAsync(LoginDTO user, AppDbContext db) {
      AppUser? u = await uMan.FindByNameAsync(user.Username);
      if(u == null) return new LoginResultDTO { Errors = ErrorMessages.UserNotFound };
      SignInResult result = await sMan.PasswordSignInAsync(u, user.Password, user.StayLoggedIn, false); // check if the password is correct
      if(!result.Succeeded) return new LoginResultDTO { Errors = ErrorMessages.InvalidPassword }; // return an error if the password is incorrect
      return new LoginResultDTO { Uid = u.Id, Errors = [], IsOrg = u.IsOrganization };
    }
    public async Task<Dictionary<string, string[]>> LogoutAsync() {
      await sMan.SignOutAsync(); // sign out the user
      return []; // return an empty list of errors
    }
    public async Task<RegisterResultDTO> RegisterAsync(RegisterUserDTO user) {
      AppUser u = new() { UserName = user.UserName, Email = user.Email, PhoneNumber = user.PhoneNumber, IsOrganization = user.IsOrg }; // create a new user
      var result = await uMan.CreateAsync(u, user.Password); // create the user
      return new() { User = u, Errors = result.Errors }; // return the result
    }
    public async Task<Dictionary<string, string[]>> SendTokenAsync(int uid, string callbackUrl, IEmailSender emailSender, TokenType type) {
      AppUser u = await uMan.FindByIdAsync(uid.ToString());
      if(u == null) return ErrorMessages.UserNotFound;
      return await SendTokenAsync(u, callbackUrl, emailSender, type);
    }
    public async Task<Dictionary<string, string[]>> SendTokenAsync(AppUser user, string callbackUrl, IEmailSender emailSender, TokenType type) {
      switch(type) {
        case TokenType.EmailConfirmation:
          callbackUrl = callbackUrl.Replace("%7B0%7D", user.Id.ToString()).Replace("%7B1%7D", await uMan.GenerateEmailConfirmationTokenAsync(user));
          await emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");
          break;
        case TokenType.PasswordReset:
          callbackUrl = callbackUrl.Replace("%7B0%7D", user.Id.ToString()).Replace("%7B1%7D", await uMan.GeneratePasswordResetTokenAsync(user));
          await emailSender.SendEmailAsync(user.Email, "Reset your password", $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");
          break;
      }
      return [];
    }
//    /// <summary>
//    /// Sends an email to one of the KYC moderators to check the user's KYC documents.
//    /// </summary>
//    /// <param name="uid">The user's ID</param>
//    /// <param name="emailSender">Mail sender to send the documents to a KYC moderator</param>
//    /// <param name="documentValue"></param>
//    /// <param name="documentType"></param>
//    /// <returns></returns>
//    public async Task<Dictionary<string, string[]>> SendKYCAsync(KYCOrg doc, IEmailSender emailSender) {
//#if DEBUG
//      // Temporary code for testing
//      AppUser u = await uMan.FindByIdAsync(doc.Uid.ToString());
//      if(u == null) return ErrorMessages.UserNotFound;
//      switch(doc.IsOrganization) {
//        case true:
//          if(doc.DocumentValue % 2 == 0) u.KYCVerified = true;
//          else {
//            u.KYCVerified = false;
//            return new() { ["DocumentValue"] = ["Document value is not even."] };
//          }
//          break;
//        case false:
//          if(doc.DocumentType) {
//            if(doc.DocumentValue % 2 == 0) u.KYCVerified = false;
//            else {
//              u.KYCVerified = true;
//              return new() { ["DocumentValue"] = ["passport value is not odd."] };
//            }
//          } else {
//            if(doc.DocumentValue % 2 == 0) u.KYCVerified = true;
//            else {
//              u.KYCVerified = false;
//              return new() { ["DocumentValue"] = ["ID card value is not even."] };
//            }
//          }
//          break;
//      }
//      await uMan.AddClaimAsync(u, new Claim(ClaimTypes.AuthenticationMethod, u.KYCVerified ? "1" : "0"));
//      await uMan.UpdateAsync(u);
//#else

//#endif
//      return [];
//    }
    public async Task<Dictionary<string, string[]>> ConfirmEmail(int userId, string code) {
      code = code.Replace(" ", "+"); // replace spaces with plus signs
      var errors = new Dictionary<string, string[]>(); // create a list of errors
      if(userId == null) errors.Add("UID", ["user ID not provided."]);
      if(code == null) errors.Add("Code", ["confirmation code not provided."]);
      if(errors.Count > 0) return errors; // return the errors if there are any

      AppUser user = await uMan.FindByIdAsync(userId.ToString());
      if(user == null)
        return ErrorMessages.UserNotFound;


      IdentityResult result = await uMan.ConfirmEmailAsync(user, code);
      return result.Succeeded ? errors : result.Errors.ToDictionary(k => k.Code, v => new string[] { v.Description });
    }
  }
}
