namespace FindMyVolunteer.Models {
  public class Favorite {
    public Favorite() { }
    public Favorite(int uid, int eventId) {
      UserId = uid;
      EventId = eventId;
    }
    public int UserId { get; set; }
    public int EventId { get; set; }
  }
}
