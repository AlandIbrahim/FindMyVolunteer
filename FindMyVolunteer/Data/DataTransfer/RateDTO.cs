using FindMyVolunteer.Models;

namespace FindMyVolunteer.Data.DataTransfer {
  public class RateDTO {
    private int _uid;
    public int Uid => _uid;
    private int _eventId;
    public int EventId => _eventId;
    public void SetID(int uid, int eventId) {
      _uid = uid;
      _eventId = eventId;
    }
    public int Rating { get; set; }
    public string? Comment { get; set; } = string.Empty;
    public static implicit operator EventRating(RateDTO rate) => new() {
      FromID = rate.Uid,
      ToID = rate.EventId,
      Rating = (byte)rate.Rating,
      Comment = rate.Comment
    };
  }
}
