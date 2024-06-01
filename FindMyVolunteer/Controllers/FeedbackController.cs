using FindMyVolunteer.Data;
using FindMyVolunteer.Data.DataTransfer;
using FindMyVolunteer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FindMyVolunteer.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class FeedbackController(AppDbContext db): ControllerBase {
    [HttpPost("{toId}")]
    public async Task<IActionResult> SetFeedback([FromBody] FeedbackDTO feedback, int toId) {
      int fromID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
      if(fromID == 0) return StatusCode(401);
      if(fromID == toId) return StatusCode(403, "You cannot give feedback to yourself.");
      Feedback f = new(fromID, toId, feedback.Rating, feedback.Comment);
      try {
        db.Feedbacks.Add(f);
        await db.SaveChangesAsync();
      } catch(DbUpdateException e) {
        SqlException sqlEx = (SqlException)e.InnerException;
        if(sqlEx == null) return StatusCode(500, $"{e.Message}\n\n\n{e.InnerException?.Message ?? ""}");
        if(sqlEx.Number == 547) return StatusCode(404, "User not found.");
        if(sqlEx.Number == 2627 || sqlEx.Number == 2601) return StatusCode(409, "You have already given feedback to this user.");
        return StatusCode(500, $"{e.Message}\n\n\n{e.InnerException?.Message ?? ""}");
      }
      return StatusCode(200);
    }
    [HttpGet("{toId}")]
    public async Task<IActionResult> GetFeedback(int toId) {
      var feedbacks = await db.Feedbacks.Where(f => f.ToID == toId).Select(f => new {
        f.From.UserName,
        f.Rating,
        f.Comment,
      }).ToListAsync();
      float avg = feedbacks.Count == 0 ? 0 : (float)feedbacks.Where(f => f.Rating != null).Average(f => f.Rating);
      return new JsonResult(new { feedbacks, avg });
    }
    [HttpPut("{toId}")]
    public async Task<IActionResult> UpdateFeedback([FromBody] FeedbackDTO feedback, int toId) {
      int fromID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
      if(fromID == 0) return StatusCode(401);
      Feedback fNew = new(fromID, toId, feedback.Rating, feedback.Comment);
      try {
        return await db.Feedbacks.Where(f => f.FromID == fromID && f.ToID == toId)
          .ExecuteUpdateAsync(f =>
            f.SetProperty(f => f.Rating, fNew.Rating)
            .SetProperty(f => f.Comment, fNew.Comment)) > 0 ?
          StatusCode(200) : StatusCode(404, "You have not given feedback to this user.");
      } catch(Exception e) { return StatusCode(500, $"{e.Message}\n\n\n{e.InnerException?.Message ?? ""}"); }
    }
    [HttpDelete("{toId}")]
    public async Task<IActionResult> RemoveFeedback(int toId) {
      int fromID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
      if(fromID == 0) return StatusCode(401);
      try {
        return await db.Feedbacks.Where(f => f.FromID == fromID && f.ToID == toId).ExecuteDeleteAsync() > 0 ? StatusCode(200) : StatusCode(404, "You have not given feedback to this user.");
      } catch(Exception e) { return StatusCode(500, $"{e.Message}\n\n\n{e.InnerException?.Message ?? ""}"); }
    }

  }
}
