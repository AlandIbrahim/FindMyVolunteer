using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Data.DataTransfer
{
    public class ResetPasswordDTO
    {
        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Code { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
