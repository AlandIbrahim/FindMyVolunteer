using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Data.DataTransfer {
  public class FeedbackDTO {
    [Required]
    [Range(1, 5)]
    public byte Rating { get; set; }
    public string Comment { get; set; } = "";
  }
}
