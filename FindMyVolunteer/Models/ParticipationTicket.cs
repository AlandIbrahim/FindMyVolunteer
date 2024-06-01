using System.ComponentModel.DataAnnotations.Schema;

namespace FindMyVolunteer.Models {
  public class ParticipationTicket {
    [ForeignKey("Event")]
    public int EventID { get; set; }
    [ForeignKey("Volunteer")]
    public int VolunteerID { get; set; }
    public bool Attended { get; set; }
    public Event Event { get; set; }
    public Volunteer Volunteer { get; set; }
  }
}
