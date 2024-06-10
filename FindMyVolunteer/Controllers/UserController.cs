using FindMyVolunteer.Data;
using FindMyVolunteer.Data.DataTransfer;
using FindMyVolunteer.Data.Types;
using FindMyVolunteer.Engines;
using FindMyVolunteer.Extensions;
using FindMyVolunteer.Models;
using FindMyVolunteer.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Net.Mail;
using System.Security.Claims;

namespace FindMyVolunteer.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class UserController(
    AppDbContext db,
    UserManager<AppUser> uMan,
    SignInManager<AppUser> sMan,
    RoleManager<AppRole> rMan,
    IEmailSender emailSender):
    ControllerBase {
    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> GetProfileInfo() {
      int uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      if(User.IsInRole("Organization")) {
        var org = await db.Organizations.Where(o => o.ID == uid).Select(o => new {
          type="o",
          uid=o.ID.ToString(),
          o.AppUser.UserName,
          o.Name,
          o.Address,
          IsGov=o.IsGovernmental
        }).FirstOrDefaultAsync();
        if(org == null) return StatusCode(404);
        return Ok(org);
      }
      var vol = await db.Volunteers.Where(v => v.ID == uid).Select(v => new {
        type="v",
        uid=v.ID.ToString(),
        v.AppUser.UserName,
        v.FirstName,
        v.MiddleName,
        v.LastName,
        year = v.Birthday.Year.ToString(),
        v.City,
        v.Gender,
      }).FirstOrDefaultAsync();
      if(vol == null) return StatusCode(404);
      return Ok(vol);
    }
    [HttpGet("role")]
    public async Task<IActionResult> GetRole() {
      int uid=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      return User.IsInRole("Organization") ? Ok("o"+uid) : Ok("v"+uid);
    }
    #region Volunteer
    [HttpPost("vol/register")]
    public async Task<IActionResult> Register([FromBody] RegisterVolunteerDTO vol) {
      using var t = await db.Database.BeginTransactionAsync();
      try {
        AppUser user = new(vol.UserName, vol.Email, vol.PhoneNumber ?? "", false);
        var registerResult = await uMan.CreateAsync(user, vol.Password);
        if(registerResult.Errors.Any()) return BadRequest(registerResult.Errors.ToDictionary(k => k.Code, v => v.Description));
        //await SendEmailConfirmationAsync(user);
        var kycClaim = new Claim("KYC", "Not verified");
        await uMan.AddClaimAsync(user, kycClaim);
        await uMan.AddToRoleAsync(user, "Volunteer");
        Volunteer volunteer = new(user.Id, vol.FirstName, vol.LastName, DateOnly.Parse(vol.Birthday), vol.Gender, Translator.FromString(vol.Languages), vol.City+1);
        db.Volunteers.Add(volunteer);
        await db.SaveChangesAsync();
        await t.CommitAsync();
        return Ok();
      } catch(InvalidOperationException) {
        await t.RollbackAsync();
        return BadRequest(DataValidator.CreateError("Email", "Email already exists."));
      } catch(SmtpException e) {
        await t.RollbackAsync();
        return StatusCode(400, DataValidator.CreateError("Email", "Email is invalid."));
      } catch {
        await t.RollbackAsync();
        return StatusCode(500);
      }
    }
    [HttpPost("vol/kyc")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> SendKYC([FromBody] KYCVolunteerDTO doc) {
      int uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      string docType = doc.IsPassport ? "Passport" : "ID Card";
      var kycClaim = User.FindFirstValue("KYC");
      AppUser user = null;
      if(kycClaim == null) {
        user = await uMan.FindByIdAsync(uid.ToString());
        kycClaim = (await uMan.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "KYC").Value;
      }
      if(kycClaim == null) await uMan.AddClaimAsync(user, new("KYC", "Pending"));
      else {
        if(kycClaim == "Verified") return StatusCode(400, "User is already verified.");
        if(kycClaim == "Pending") return StatusCode(400, "KYC is already pending.");
      }
      var admin = await db.Admins.OrderBy(a => a.UsersValidated).Select(a => new { a.FirstName, a.LastName, a.User.Email }).FirstOrDefaultAsync();
      if(admin == null) return StatusCode(500);
      await emailSender.SendEmailAsync(admin.Email!, $"KYC for {User.Identity!.Name}", $"""
<h1>Dear {admin.FirstName} {admin.LastName}.</h1>
<br>
Please review the KYC document for <span style='font-weight: bold;'>{User.Identity.Name}</span> and validate the user, here is the document:
<br>
<span style='font-weight: bold;'>{docType}</span>
<img src='{doc.DocumentNumber}' alt='{docType}' style='max-width: 100%; max-height: 100%;'>
<br>
<br>
To validate the user by clicking <a href='{Url.Action("Validate/User?userId=" + uid, "Admin", Request.Scheme)}'>here</a>, or reject them by clicking <a href='{Url.Action("Validate/User?userId=" + uid + "&reject=true", "Admin", Request.Scheme)}'>here</a>.
""");
      return Ok();
    }
    #endregion
    #region Organization
    [HttpPost("org/register")]
    public async Task<IActionResult> Register([FromBody] RegisterOrganizationDTO org) {
      using var t = await db.Database.BeginTransactionAsync();
      try {
        UserEngine uEngine = new(uMan, sMan);
        // register the user
        AppUser user = org;
        IdentityResult registerResult = await uMan.CreateAsync(user, org.Password);
        if(registerResult.Errors.Any()) return BadRequest(registerResult.Errors);
        await uMan.AddToRoleAsync(user, "Organization");
        // email confirmation
        //await SendEmailConfirmationAsync(user);
        // KYC
        if(org.BusinessLicense != null) {
          await SendKYC(int.Parse(org.BusinessLicense),user);
          await uMan.AddClaimAsync(user, new Claim("KYC", "Pending"));
        } else await uMan.AddClaimAsync(user, new Claim("KYC", "Not verified"));
        // create the organization
        Organization organization = new() {
          ID = user.Id,
          Name = org.Name,
          Address = org.Address,
          BusinessLicense = org.BusinessLicense ?? "",
          IsGovernmental = org.IsGovernmental
        };
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();
        await t.CommitAsync();
        return Ok();
      } catch(InvalidOperationException) {
        await t.RollbackAsync();
        return BadRequest(DataValidator.CreateError("Email", "Email already exists."));
      } catch {
        await t.RollbackAsync();
        return StatusCode(500);
      }
    }
    [HttpPost("org/kyc")]
    [Authorize(Roles = "Organization")]
    public async Task<IActionResult> SendKYC([FromBody] int doc,int uid=0) {
      uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      AppUser user = await uMan.FindByIdAsync(uid.ToString());
      var kycClaim = (await uMan.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "KYC");
      if(kycClaim == null) await uMan.AddClaimAsync(user, new("KYC", "Pending"));
      else {
        if(kycClaim.Value == "Verified") return StatusCode(400, "User is already verified.");
        if(kycClaim.Value == "Pending") return StatusCode(400, "KYC is already pending.");
      }
      var admin = await db.Admins.OrderBy(a => a.UsersValidated).Select(a => new { a.FirstName, a.LastName, a.User.Email }).FirstOrDefaultAsync();
      if(admin == null) return StatusCode(500);
      await emailSender.SendEmailAsync(admin.Email!, $"KYC for {User.Identity!.Name}", $"""
<h1>Dear {admin.FirstName} {admin.LastName}.</h1>
<br>
Please review the KYC document for <span style='font-weight: bold;'>{User.Identity.Name}</span> and validate the user, here is the document:
<br>
<span style='font-weight: bold;'>Business License</span>
<img src='{doc}' alt='Business License' style='max-width: 100%; max-height: 100%;'>
<br>
<br>
To validate the user by clicking <a href='{Url.Action("Validate/User?userId=" + uid, "Admin", Request.Scheme)}'>here</a>, or reject them by clicking <a href='{Url.Action("Validate/User?userId=" + uid + "&reject=true", "Admin", Request.Scheme)}'>here</a>.
""");
      return Ok();
    }
    private async Task SendKYC(int doc, AppUser user) {
      var admin = await db.Admins.OrderBy(a => a.UsersValidated).Select(a => new { a.FirstName, a.LastName, a.User.Email }).FirstOrDefaultAsync() ?? throw new InvalidOperationException("No admins found.");
      //await emailSender.SendEmailAsync(admin.Email!, $"KYC for {User.Identity!.Name}", $"""
      //  <h1>Dear {admin.FirstName} {admin.LastName}.</h1>
      //  <br>
      //  Please review the KYC document for <span style='font-weight: bold;'>{User.Identity.Name}</span> and validate the user, here is the document:
      //  <br>
      //  <span style='font-weight: bold;'>Business License</span>
      //  <img src='{doc}' alt='Business License' style='max-width: 100%; max-height: 100%;'>
      //  <br>
      //  <br>
      //  To validate the user by clicking <a href='{Url.Action("Validate/User?userId=" + user.Id, "Admin", Request.Scheme)}'>here</a>, or reject them by clicking <a href='{Url.Action("Validate/User?userId=" + user.Id + "&reject=true", "Admin", Request.Scheme)}'>here</a>.
      //  """);
    }
    #endregion
    #region Admin
    [HttpGet("admin/validate/{userId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Validate(int userId) {
      using var t = await db.Database.BeginTransactionAsync();
      try {
        AppUser user = await db.Users.FindAsync(userId);
        if(user == null) return NotFound();
        var kycClaim = (await uMan.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "KYC");
        if(kycClaim != null) {
          if(kycClaim.Value != "Verified") await uMan.ReplaceClaimAsync(user, kycClaim, new Claim("KYC", "Verified"));
          else return StatusCode(400, "User is already verified.");
        } else await uMan.AddClaimAsync(user, new Claim("KYC", "Verified"));
        await t.CommitAsync();
        return Ok();
      } catch {
        await t.RollbackAsync();
        return new StatusCodeResult(500);
      }
    }
    #endregion
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO user) {
      AppUser? u = await uMan.FindByNameAsync(user.Username);
      Microsoft.AspNetCore.Identity.SignInResult result = await sMan.PasswordSignInAsync(u, user.Password, user.StayLoggedIn, false); // check if the password is correct
      if(!result.Succeeded) return Unauthorized(); // return an error if the password is incorrect
      return Ok(User.IsInRole("Organization")?"o"+u.Id:"v"+u.Id);
    }
    [HttpGet("logout")]
    public async Task<IActionResult> Logout() {
      _ = await new UserEngine(uMan, sMan).LogoutAsync();
      return Ok();
    }
    #region token actions
    [HttpPost("passwordreset/send")]
    public async Task<IActionResult> SendPasswordReset([FromBody] string email) {
      var user = await uMan.FindByEmailAsync(email);
      if(user == null) return StatusCode(404, "User not found.");
      //var errors = await new UserEngine(uMan, sMan).SendTokenAsync(uid, HttpContext.Request.Scheme + "://" +
      //  HttpContext.Request.Host + Url.Page("/ResetPassword", new { userId = "{0}", code = "{1}" }), emailSender, TokenType.PasswordReset);
      try {
        var callbackUrl = Url.Page(HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "/ResetPassword", new { userId = user.Id, code = await uMan.GeneratePasswordResetTokenAsync(user) });
        await emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");
      } catch(Exception e) { return StatusCode(500, e.Message); }
      return Ok();
    }
    [HttpPost("emailconfirmation/send")]
    [Authorize]
    public async Task<IActionResult> SendEmailConfirmation() {
      try {
        var user = await uMan.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        await SendEmailConfirmationAsync(user);
      } catch(Exception e) { return StatusCode(500, e.Message); }
      return Ok();
    }
    private async Task SendEmailConfirmationAsync(AppUser user) {
      string callbackUrl = Url.Action("emailconfirmation/confirm", "User", new { userId = user.Id, code = await uMan.GenerateEmailConfirmationTokenAsync(user) }, Request.Scheme);
      await emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");
    }
    [HttpGet("emailconfirmation/confirm")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string code) {
      try {
        var user = await uMan.FindByIdAsync(userId.ToString());
        var result = await uMan.ConfirmEmailAsync(user, code.Replace(" ", "+"));
        return result.Succeeded ? Ok() : StatusCode(400, result.Errors.ToDictionary(k => k.Code, v => v.Description));
      } catch(Exception e) { return StatusCode(500, e.Message); }
    }
    #endregion
    #region PfP
    [HttpPost("pfp")]
    [Authorize]
    [RequestSizeLimit(0x80_0000)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile img) {
      int uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      try {
        string pName = "pfp";
        pName += Path.GetExtension(img.FileName);
        string path = Path.Combine(Directory.GetCurrentDirectory(), "img", uid.ToHex(8));
        if(!Directory.Exists(path)) Directory.CreateDirectory(path);
        using FileStream fs = new(Path.Combine(path, pName), FileMode.Create);
        await img.CopyToAsync(fs);
        return Ok();
      } catch(Exception e) { return StatusCode(500, e.Message); }
    }
    [HttpPatch("pfp")]
    [Authorize]
    [RequestSizeLimit(0x80_0000)]
    public async Task<IActionResult> UpdateProfilePicture(IFormFile img) {
      int uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      try {
        string pName = "pfp";
        pName += Path.GetExtension(img.FileName);
        string path = Path.Combine(Directory.GetCurrentDirectory(), "img", uid.ToHex(8));
        if(!Directory.Exists(path)) Directory.CreateDirectory(path);
        using FileStream fs = new(Path.Combine(path, pName), FileMode.Create);
        await img.CopyToAsync(fs);
        return Ok();
      } catch(Exception e) { return StatusCode(500, e.Message); }
    }
    [HttpGet("pfp/{uid}")]
    public async Task<IActionResult> GetCurrentProfilePicture(int uid) {
      //string fileName = Path.Combine(Directory.GetCurrentDirectory(), "img", uid.ToHex(8), "U" + imgId.ToHex(8) + ".*");
      string fileName=Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "img", uid.ToHex(8))).FirstOrDefault(f => f.Contains("pfp"));
      if(fileName == null) return StatusCode(500);
      using FileStream fs = new(fileName, FileMode.Open);
      byte[] data = new byte[fs.Length];
      await fs.ReadAsync(data);
      return File(data, "image/" + Path.GetExtension(fileName)[1..]);
    }
    #endregion
  }
}
