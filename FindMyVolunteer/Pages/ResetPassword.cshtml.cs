using FindMyVolunteer.Data.DataTransfer;
using FindMyVolunteer.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FindMyVolunteer.Pages
{
    public class ResetPasswordModel(UserManager<AppUser> uMan): PageModel {
    [BindProperty(SupportsGet =true)]
    public ResetPasswordDTO Input { get; set; }
    public void OnGet() {
      Console.WriteLine(Input.UserId);
    }
    public async Task<IActionResult> OnPostAsync() {
      if(!ModelState.IsValid) {
        return Page();
      }

      var user = await uMan.FindByIdAsync(Input.UserId.ToString());
      if(user == null) {
        // Don't reveal that the user does not exist
        return RedirectToPage("./ResetPasswordConfirmation");
      }

      var result = await uMan.ResetPasswordAsync(user, Input.Code.Replace(' ','+'), Input.Password);
      if(result.Succeeded) {
        return RedirectToPage("./ResetPasswordConfirmation");
      }

      foreach(var error in result.Errors) {
        ModelState.AddModelError(string.Empty, error.Description);
      }
      return Page();
    }
  }
}
