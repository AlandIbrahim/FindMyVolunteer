using FindMyVolunteer.Data.Types;
using FindMyVolunteer.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FindMyVolunteer.Data.DataTransfer {
  public class CreateEventDTO {
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    private DateTime? _enrollmentDeadline;
    [JsonIgnore]
    public DateTime DtEnrollmentDeadline { get => _enrollmentDeadline ?? _startDate;}
    public string EnrollmentDeadline { get => (_enrollmentDeadline ?? _startDate).ToString(); set => _enrollmentDeadline = DateTime.Parse(value); }
    private DateTime _startDate;
    [JsonIgnore]
    public DateTime DtStartDate { get => _startDate; }
    public string StartDate { get => _startDate.ToString(); set => _startDate = DateTime.Parse(value); }
    [RegularExpression(@"\d{2}:\d{2}", ErrorMessage = "Duration must be in the format HH:mm")]
    public string Duration { get; set; }
    [JsonIgnore]
    public DateTime EndDate => _startDate.Add(new TimeSpan(int.Parse(Duration.Split(':')[0]), int.Parse(Duration.Split(':')[1]), 0));

    public string Location { get; set; } = string.Empty;
    public City City { get; set; }
    public List<int> Skills { get; set; } = [];
    public short MaxAttendees { get; set; } = 1;
    public string? Image { get; set; }
    public static Event Convert(CreateEventDTO e, int orgID) => new() {
      Title = e.Title,
      Description = e.Description,
      OrganizationID = orgID,
      EnrollmentDeadline = e.DtEnrollmentDeadline,
      StartDate = e.DtStartDate,
      Duration = TimeSpan.Parse(e.Duration),
      Location = e.Location,
      City = e.City,
      MaxAttendees = e.MaxAttendees
    };
    public static Event Update(Event @base, CreateEventDTO @event) {
      @base.Title = @event.Title;
      @base.Description = @event.Description;
      @base.EnrollmentDeadline = @event._enrollmentDeadline??@event._startDate;
      @base.StartDate = @event._startDate;
      @base.Duration = TimeSpan.Parse(@event.Duration);
      @base.Location = @event.Location;
      @base.City = @event.City;
      @base.MaxAttendees = @event.MaxAttendees;
      return @base;
    }
    public static explicit operator CreateEventDTO(Event e) => new() {
      Title = e.Title,
      Description = e.Description,
      _enrollmentDeadline = e.EnrollmentDeadline,
      _startDate = e.StartDate,
      Duration= e.Duration.ToString(),
      Location = e.Location,
      City = e.City,
      MaxAttendees = e.MaxAttendees
    };
    public static implicit operator Event(CreateEventDTO e) => new() {
      Title = e.Title,
      Description = e.Description,
      EnrollmentDeadline = e.DtEnrollmentDeadline,
      StartDate = e.DtStartDate,
      Duration = TimeSpan.Parse(e.Duration),
      Location = e.Location,
      City = e.City,
      MaxAttendees = e.MaxAttendees
    };
  }
}
