using FindMyVolunteer.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace FindMyVolunteer.Data.DataTransfer
{
    public class RegisterResultDTO
    {
        public AppUser? User { get; set; }
        public IEnumerable<IdentityError> Errors { get; set; } = new List<IdentityError>();
    }
}
