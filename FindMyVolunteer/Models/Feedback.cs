using FindMyVolunteer.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FindMyVolunteer.Models {
  public class Feedback {
    public Feedback() { }
    public Feedback(int fromID, int toID, byte rating, string? comment) {
      FromID = fromID;
      ToID = toID;
      Rating = rating;
      Comment = comment;
    }
    [Key]
    [ForeignKey("From")]
    public int FromID { get; set; }
    public AppUser? From { get; set; }
    [Key]
    [ForeignKey("To")]
    public int ToID { get; set; }
    public AppUser? To { get; set; }
    [Range(1, 5)]
    public byte Rating { get; set; }
    [StringLength(1024)]
    public string? Comment { get; set; }
  }
}
