using FindMyVolunteer.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FindMyVolunteer.Models {
  public class Organization {
    [ForeignKey("AppUser")]
    public int ID { get; set; }
    public AppUser? AppUser { get; set; }
    [Required]
    [StringLength(64)]
    public string Name { get; set; }
    [Required]
    [StringLength(64)]
    public string Address { get; set; }
    [Required]
    [StringLength(64)]
    public string BusinessLicense { get; set; }
    [Required]
    public bool IsGovernmental { get; set; }

  }
}
