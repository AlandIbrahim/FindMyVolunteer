using FindMyVolunteer.Data;
using FindMyVolunteer.Data.DataTransfer;
using FindMyVolunteer.Data.Types;
using FindMyVolunteer.Engines;
using FindMyVolunteer.Extensions;
using FindMyVolunteer.Models;
using FindMyVolunteer.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FindMyVolunteer.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class EventController(AppDbContext db, IEmailSender emailSender, UserManager<AppUser> uMan): ControllerBase {
    #region Retreival
    /// <summary>
    /// <para>[<see cref="Volunteer"/>]Get all the <see cref="Event">events</see> that the volunteer has participated in.</para>
    /// <para>[<see cref="Organization"/>]Get all the <see cref="Event">events</see> that the organization has hosted.</para>
    /// </summary>
    /// <param name="includePast">Include past <see cref="Event">events</see>.</param>
    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> GetEvents([FromQuery] bool includePast = true, int page = 1) {
      int id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
      return User.FindFirst(ClaimTypes.Role)!.Value switch {
        "Volunteer" => Ok(await db.ParticipationTickets.Where(t => t.VolunteerID == id)
                 .Join(db.Organizations, e => e.Event.OrganizationID, o => o.ID, (t, o) => new { t, o })
                 .Select(j => new {
                   j.t.Event.ID,
                   j.t.Event.Title,
                   Organization = j.o.Name,
                   j.t.Event.City,
                   j.t.Event.Location,
                   j.t.Event.EnrollmentDeadline,
                   j.t.Event.StartDate,
                   j.t.Event.Duration,
                   Status = Translator.EventStatus(
                     j.t.Event.EnrollmentDeadline,
                     j.t.Event.StartDate,
                     j.t.Event.EndDate,
                     true,
                     j.t.Attended,
                     j.t.Event.Cancelled,
                     j.t.Event.CurrentAttendees >= j.t.Event.MaxAttendees
                   ),
                 }).OrderBy(e => e.EnrollmentDeadline).ThenBy(e => e.StartDate).Skip((page - 1) * 5).Take(5).ToListAsync()),
        "Organization" => Ok(await db.Events.Where(e => e.OrganizationID == id)
            .Select(e => new {
              e.ID,
              e.Title,
              oid = e.OrganizationID,
              Organization = e.Organization.Name,
              e.City,
              e.Location,
              e.EnrollmentDeadline,
              e.StartDate,
              e.Duration,
              Status = Translator.EventStatus(
                e.EnrollmentDeadline,
                e.StartDate,
                e.EndDate,
                e.Cancelled,
                e.CurrentAttendees >= e.MaxAttendees
              ),
            }).OrderBy(e => e.EnrollmentDeadline).ThenBy(e => e.StartDate).Skip((page - 1) * 5).Take(5).ToListAsync()),
        _ => Forbid(),
      };
    }
    /// <summary>
    /// Lists all the <see cref="Event">events</see>, regardless of the <see cref="Models.Identity.AppUser">user</see>'s participation.
    /// </summary>
    /// <param name="includePast">Include past <see cref="Event">events</see>.</param>
    [HttpGet("all")]
    public async Task<IActionResult> ListEvents(int page = 1, bool nearMe = true) {
      City? city = null;
      nearMe &= int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int uid);
      return Ok(await db.Events
        .Where(e => !e.Cancelled && (!nearMe || e.City == (city ?? City.Sulaymaniyah)))
        .Join(db.Organizations, e => e.OrganizationID, o => o.ID, (e, o) => new { e, o })
        .Select(j => new {
          j.e.ID,
          j.e.Title,
          oid = j.e.OrganizationID,
          Organization = j.o.Name,
          j.e.City,
          j.e.Location,
          j.e.EnrollmentDeadline,
          j.e.StartDate,
          j.e.Duration,
          Status = Translator.EventStatus(
            j.e.EnrollmentDeadline,
            j.e.StartDate,
            j.e.EndDate,
            j.e.Cancelled,
            j.e.CurrentAttendees >= j.e.MaxAttendees
          ),
        }).OrderBy(e => e.EnrollmentDeadline).ThenBy(e => e.StartDate).Skip((page - 1) * 5).Take(5).ToListAsync());
    }
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search, int page = 1) {
      var events = await db.Events.Where(e => e.Title.Contains(search) || e.Description.Contains(search))
        .Join(db.Organizations, e => e.OrganizationID, o => o.ID, (e, o) => new { e, o })
        .Select(j => new {
          j.e.ID,
          j.e.Title,
          Oid = j.e.OrganizationID,
          Organization = j.o.Name,
          j.e.City,
          j.e.Location,
          j.e.EnrollmentDeadline,
          j.e.StartDate,
          j.e.Duration,
          status= User.Identity.IsAuthenticated?Translator.EventStatus(
            j.e.EnrollmentDeadline,
            j.e.StartDate,
            j.e.EndDate,
            db.ParticipationTickets.Where(t => t.VolunteerID == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value) && t.EventID == j.e.ID).Any(),
            db.ParticipationTickets.Where(t => t.VolunteerID == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value) && t.EventID == j.e.ID).Select(t => t.Attended).FirstOrDefault(),
            j.e.Cancelled,
            j.e.CurrentAttendees >= j.e.MaxAttendees
            )
          :Translator.EventStatus(
            j.e.EnrollmentDeadline,
            j.e.StartDate,
            j.e.EndDate,
            j.e.Cancelled,
            j.e.CurrentAttendees >= j.e.MaxAttendees
          )
        }).OrderBy(e => e.EnrollmentDeadline).ThenBy(e => e.StartDate).Skip((page - 1) * 5).Take(5).ToListAsync();
      var vols = await db.Volunteers.Where(v => v.FirstName.Contains(search) || (v.MiddleName ?? "").Contains(search) || v.LastName.Contains(search) || v.AppUser.UserName.Contains(search))
        .Select(v => new {
          v.ID,
          v.FirstName,
          v.MiddleName,
          v.LastName,
        }).Skip((page - 1) * 5).Take(5).ToListAsync();
      var orgs = await db.Organizations.Where(o => o.Name.Contains(search) || o.AppUser.UserName.Contains(search))
        .Select(o => new {
          o.ID,
          o.Name,
        }).Skip((page - 1) * 5).Take(5).ToListAsync();
      return Ok(new { events, vols, orgs });
    }
    /// <summary>
    /// <para>[<c>any</c>] Details of the event, including Title, Organization, Location, Date, and City.</para>
    /// <para>[<see cref="Volunteer"/>] also includes the participation status.</para>
    /// <para>[<see cref="Organization"/>] also includes the volunteers that have enrolled.</para>
    /// </summary>
    /// <param name="id">the <see cref="Event.ID">event ID</see>.</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(int id, [FromQuery] bool forEdit) {

      _ = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int uid);
      if(User.IsInRole("Volunteer"))
        return Ok(await db.Events.Where(e => e.ID == id).GroupJoin(db.ParticipationTickets, e => e.ID, t => t.EventID, (e, t) => new { e, t })
        .Join(db.Organizations, j => j.e.OrganizationID, o => o.ID, (j, o) => new { j.t, j.e, o }).Select(j => new {
          j.e.ID,
          j.e.Title,
          OrgId = j.o.ID,
          Organization = j.o.Name,
          j.e.City,
          j.e.Location,
          j.e.EnrollmentDeadline,
          j.e.StartDate,
          j.e.Duration,
          j.e.Description,
          Status = Translator.EventStatus(
            j.e.EnrollmentDeadline,
            j.e.StartDate,
            j.e.EndDate,
            j.t.Where(t => t.VolunteerID == uid).Any(),
            j.t.Where(t => t.VolunteerID == uid).Select(t => t.Attended).FirstOrDefault(),
            j.e.Cancelled,
            j.e.CurrentAttendees >= j.e.MaxAttendees
          )
        }).FirstOrDefaultAsync());
      if(User.IsInRole("Organization")) {
        var result = await db.Events.Where(e => e.ID == id).GroupJoin(db.ParticipationTickets, e => e.ID, t => t.EventID, (e, t) => new { e, t })
        .Join(db.Organizations, j => j.e.OrganizationID, o => o.ID, (j, o) => new { j.t, j.e, o })
        .Select(j => new {
          j.e.ID,
          j.e.Title,
          OrgId = j.o.ID,
          Organization = j.o.Name,
          j.e.City,
          j.e.Location,
          j.e.EnrollmentDeadline,
          j.e.StartDate,
          j.e.Duration,
          j.e.Description,
          j.e.MaxAttendees,
          skills = db.RequiredSkills.Where(es => es.EventID == id).Select(es => es.Skill.Name).ToList(),
          Status = Translator.EventStatus(
            j.e.EnrollmentDeadline,
            j.e.StartDate,
            j.e.EndDate,
            j.e.Cancelled,
            j.e.CurrentAttendees >= j.e.MaxAttendees
            ),
          Volunteers = User.FindFirst(ClaimTypes.NameIdentifier)!.Value == j.o.ID.ToString() ?
          db.ParticipationTickets
          .Where(t => t.EventID == id)
          .Join(db.Volunteers, t => t.VolunteerID, v => v.ID, (t, v) => new { t, v })
          .Select(j => j.v.FirstName + " " + j.v.LastName)
          .ToList() : null
        }).FirstOrDefaultAsync();
        if(forEdit && result.OrgId.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)!.Value) return StatusCode(403, "Not the owner");
        return Ok(result);
      }
      return Ok(await db.Events.Where(e => e.ID == id).Join(db.Organizations, e => e.OrganizationID, o => o.ID, (e, o) => new { e, o })
      .Select(j => new {
        j.e.ID,
        j.e.Title,
        OrgId = j.o.ID,
        Organization = j.o.Name,
        j.e.City,
        j.e.Location,
        j.e.EnrollmentDeadline,
        j.e.StartDate,
        j.e.Duration,
        j.e.Description,
        Status = Translator.EventStatus(
          j.e.EnrollmentDeadline,
          j.e.StartDate,
          j.e.EndDate,
          j.e.Cancelled,
          j.e.CurrentAttendees >= j.e.MaxAttendees
          ),
      }).FirstOrDefaultAsync());
    }
    #endregion
    /// <summary>
    /// Host an <see cref="Event"/>."/>
    /// </summary>
    /// <param name="e">The <see cref="CreateEventDTO">event details</see>.</param>
    [HttpPost("host")]
    [Authorize(Roles = "Admin, Organization")]
    public async Task<IActionResult> HostEvent([FromBody] CreateEventDTO e) {
      var kycClaim = (await uMan.GetClaimsAsync(await uMan.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)!.Value))).FirstOrDefault(c => c.Type == "KYC");
      if(kycClaim == null || kycClaim.Value != "Verified") return StatusCode(403, "Organization has not done the KYC");
      Event @event = CreateEventDTO.Convert(e, int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
      db.Events.Add(@event);
      await db.SaveChangesAsync();
      foreach(var skill in e.Skills) {
        db.RequiredSkills.Add(new() { EventID = @event.ID, SkillID = skill });
      }
      int uid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
      string path = Path.Combine(Directory.GetCurrentDirectory(), "img", uid.ToHex(8));
      if(!Directory.Exists(path)) Directory.CreateDirectory(path);
      await db.SaveChangesAsync();
      if(e.Image != null) {
        string ext = Path.GetExtension(e.Image);
        string imgPath = Path.Combine(path, @event.ID.ToHex(8) + ext);
        //rename the file to the event id
        string fullPath = Directory.GetFiles(path).FirstOrDefault(f => f.Contains(e.Image));
        System.IO.File.Move(fullPath, imgPath);
      }
      return Ok();
    }
    [HttpPost("upload")]
    [Authorize(Roles = "Admin, Organization")]
    [RequestSizeLimit(80_000_000)]
    public async Task<IActionResult> UploadImage(IFormFile file) {
      if(file == null) return StatusCode(400);
      int uid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
      string path = Path.Combine(Directory.GetCurrentDirectory(), "img", uid.ToHex(8));
      if(!Directory.Exists(path)) Directory.CreateDirectory(path);
      string fileName = DateTime.Now.Ticks + Path.GetExtension(file.FileName);
      string imgPath = Path.Combine(path, fileName);
      using(var stream = new FileStream(imgPath, FileMode.Create)) {
        await file.CopyToAsync(stream);
      }
      return Ok(fileName);
    }
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadImage(int id) {
      int uid = await db.Events.Where(e => e.ID == id).Select(e => e.OrganizationID).FirstOrDefaultAsync();
      string path = Path.Combine(Directory.GetCurrentDirectory(), "img", uid.ToHex(8));
      if(!Directory.Exists(path)) return StatusCode(404);
      string[] files = Directory.GetFiles(path);
      string hexId = id.ToHex(8);
      string file = files.FirstOrDefault(f => f.Contains(id.ToHex(8)));
      if(file == null) return StatusCode(404);
      return PhysicalFile(file, "image/jpeg");
    }
    #region Update
    /// <summary>Update the <see cref="Event"/> details.</summary>
    /// <param name="e">The updated <see cref="Event"/> details.</param>
    /// <param name="id">The <see cref="Event.ID">event ID</see></param>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin, Organization")]
    public async Task<IActionResult> UpdateEvent([FromBody] CreateEventDTO e, int id) {
      //if(User.FindFirst(ClaimTypes.AuthenticationMethod)?.Value != "1") return StatusCode(403, "Organization has not done the KYC");
      var kycClaim = (await uMan.GetClaimsAsync(await uMan.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)!.Value))).FirstOrDefault(c => c.Type == "KYC");
      if(kycClaim == null || kycClaim.Value != "Verified") return StatusCode(403, "Organization has not done the KYC");
      Event? @event = await db.Events.FindAsync(id);
      if(@event == null) return StatusCode(404);
      if(@event.OrganizationID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)) return StatusCode(403);
      CreateEventDTO.Update(@event, e);
      db.Events.Update(@event);
      await db.SaveChangesAsync();
      return Ok();
    }
    #endregion
    /// <summary>Cancel an <see cref="Event"/>.</summary>
    /// <param name="id">The <see cref="Event.ID">event ID</see></param>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, Organization")]
    public async Task<IActionResult> CancelEvent(int id) {
      Event @event = await db.Events.FindAsync(id);
      if(@event == null) return NotFound();
      if(@event.OrganizationID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)) return Forbid("Not the owner");
      var volunteerEmails = await db.ParticipationTickets.Join(db.Users, t => t.VolunteerID, u => u.Id, (t, u) => new { t, u })
        .Where(j => j.t.EventID == id).Select(j => j.u.Email).ToListAsync();
      List<Task> tasks = [];
      foreach(var email in volunteerEmails) {
        // send email to volunteers
        tasks.Add(emailSender.SendEmailAsync(email, "Event Cancelled", $"The event \"{@event.Title}\" has been cancelled."));
      }
      await Task.WhenAll(tasks);
      await db.Events.Where(e => e.ID == id).ExecuteUpdateAsync(e => e.SetProperty(x => x.Cancelled, true));
      return Ok();
    }
    #region Enrollment
    /// <summary>Enroll into an <see cref="Event"/>.</summary>
    /// <param name="id">The <see cref="Event.ID">event ID</see></param>
    [HttpGet("{id}/enroll")]
    [Authorize(Roles = "Admin, Volunteer")]
    public async Task<IActionResult> Enroll(int id) {
      var @event = await db.Events.FindAsync(id);
      if(@event == null) return StatusCode(404);
      if(@event.Cancelled) return StatusCode(403, "Event cancelled");
      int uid = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
      if(await db.ParticipationTickets.Where(t => t.VolunteerID == uid && t.EventID == id).AnyAsync()) return StatusCode(403, "Already enrolled");

      if(@event.CurrentAttendees >= @event.MaxAttendees) return StatusCode(403, "Event is full");

      db.ParticipationTickets.Add(new() { VolunteerID = uid, EventID = id });
      @event.CurrentAttendees++;
      db.Events.Update(@event);
      await db.SaveChangesAsync();
      return Ok();
    }
    /// <summary>Unenroll from an <see cref="Event"/>.</summary>
    /// <param name="id">The <see cref="Event.ID">event ID</see></param>
    [HttpGet("{id}/unenroll")]
    [Authorize(Roles = "Admin, Volunteer")]
    public async Task<IActionResult> Unenroll(int id) {
      var @event = await db.Events.FindAsync(id);
      if(@event == null) return StatusCode(404);
      if(@event.Cancelled) return StatusCode(403, "Event cancelled");
      int uid = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
      var ticket = await db.ParticipationTickets.Where(t => t.VolunteerID == uid && t.EventID == id).FirstOrDefaultAsync();
      if(ticket == null) return StatusCode(403, "Not enrolled");

      db.ParticipationTickets.Remove(ticket);
      @event.CurrentAttendees--;
      db.Events.Update(@event);
      await db.SaveChangesAsync();
      return Ok();
    }
    #endregion
    #region Favoriting
    /// <summary>Add an <see cref="Event"/> to the user's favorites.</summary>
    /// <param name="id">The <see cref="Event.ID">event ID</see></param>
    [HttpGet("{id}/Favorite")]
    [Authorize]
    public async Task<IActionResult> Favorite(int id) {
      var @event = await db.Events.FindAsync(id);
      if(@event == null) return StatusCode(404);
      if(@event.Cancelled) return StatusCode(403, "Event cancelled");
      int uid = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
      if(await db.Favorites.Where(f => f.UserId == uid && f.EventId == id).AnyAsync()) return StatusCode(403, "Already favorited");

      db.Favorites.Add(new(uid, id));
      await db.SaveChangesAsync();
      return Ok();
    }
    /// <summary>Remove an <see cref="Event"/> from the user's favorites.</summary>
    /// <param name="id">The <see cref="Event.ID">event ID</see></param>
    [HttpGet("{id}/Unfavorite")]
    [Authorize]
    public async Task<IActionResult> Unfavorite(int id) {
      var @event = await db.Events.FindAsync(id);
      if(@event == null) return StatusCode(404);
      if(@event.Cancelled) return StatusCode(403, "Event cancelled");
      int uid = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
      var favorite = await db.Favorites.Where(f => f.UserId == uid && f.EventId == id).FirstOrDefaultAsync();
      if(favorite == null) return StatusCode(403, "Not favorited");

      db.Favorites.Remove(favorite);
      await db.SaveChangesAsync();
      return Ok();
    }
    #endregion
    #region Ratings
    [HttpGet("{id}/rate")]
    public async Task<IActionResult> GetRating(int id) {
      var ratings = await db.EventRatings.Where(r => r.ToID == id).ToListAsync();

      try {
        var counts = new {
          v = ratings.Count(r => r.Rating == 5),
          iv = ratings.Count(r => r.Rating == 4),
          iii = ratings.Count(r => r.Rating == 3),
          ii = ratings.Count(r => r.Rating == 2),
          i = ratings.Count(r => r.Rating == 1),
          total = ratings.Count == 0 ? 1 : ratings.Count
        };
        var uid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var rateObj = new {
          ratings,
          self = ratings.Where(r => r.FromID == uid).Select(r => r.Rating).FirstOrDefault(),
          counts
        };
        return Ok(rateObj);
      } catch(Exception e) {
        return Ok(new { });
        #endregion
      }
    }
    [HttpPost("{id}/rate")]
    [Authorize]
    public async Task<IActionResult> AddRating([FromBody] RateDTO rating, int id) {
      var @event = await db.Events.Where(e => e.ID == id).Select(e => new { e.EndDate, e.Cancelled }).FirstOrDefaultAsync();
      if(@event == null) return NotFound();
      if(@event.Cancelled) return Forbid("Event cancelled");
      if(@event.EndDate > DateTime.Now) return Forbid("Event has not ended yet");
      int uid = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
      if(User.IsInRole("Volunteer") && await db.ParticipationTickets.Where(t => t.VolunteerID == uid && t.EventID == rating.EventId).Select(t => t.Attended).FirstOrDefaultAsync()) return Forbid("Cannot rate an event you did not attend");
      rating.SetID(uid, id);
      try {
        db.EventRatings.Add(rating);
        await db.SaveChangesAsync();
      } catch(Exception e) {
        return StatusCode(500, e.Message);
      }
      return Ok();
    }
    [HttpPatch("{id}/rate")]
    [Authorize]
    public async Task<IActionResult> UpdateRating([FromBody] RateDTO rating, int id) {
      int uid = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
      rating.SetID(uid, id);
      try {
        db.EventRatings.Update(rating);
        await db.SaveChangesAsync();
      } catch(Exception e) {
        return StatusCode(500, e.Message);
      }
      return Ok();
    }
    [HttpDelete("{id}/rate")]
    [Authorize]
    public async Task<IActionResult> DeleteRating(int id) {
      int result;
      int uid = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
      try {
        result = await db.EventRatings.Where(r => r.FromID == uid && r.ToID == id).ExecuteDeleteAsync();
      } catch(Exception e) {
        return StatusCode(500, e.Message);
      }
      if(result == 0) return NotFound();
      return Ok();
    }
  }
}