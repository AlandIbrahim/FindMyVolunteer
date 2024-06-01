using FindMyVolunteer.Data.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FindMyVolunteer.Models {
  public class Event {
    [Key]
    public int ID { get; set; }
    [Required]
    [StringLength(32)]
    public string Title { get; set; } = string.Empty;
    [StringLength(1024)]
    public string Description { get; set; } = string.Empty;
    [Required]
    [ForeignKey("Organization")]
    public int OrganizationID { get; set; }
    public Organization Organization { get; set; }
    private DateTime? _enrollmentDeadline;
    public DateTime EnrollmentDeadline { get => _enrollmentDeadline ?? StartDate; set => _enrollmentDeadline = value; }
    private DateTime _startDate;
    [Required]
    public DateTime StartDate { get => _startDate; set => _startDate = value; }
    private TimeSpan _duration;
    [Required]
    public TimeSpan Duration {get => _duration; set => _duration = value;}
    public DateTime EndDate => StartDate + Duration;
    [Required]
    [StringLength(64)]
    public string Location { get; set; } = string.Empty;
    [Required]
    public City City { get; set; }
    public short CurrentAttendees { get; set; } = 0;
    [Required]
    [Range(1, short.MaxValue)]
    public short MaxAttendees { get; set; } = 1;
    public bool Cancelled { get; set; }
  }
}
