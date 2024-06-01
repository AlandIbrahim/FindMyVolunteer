using FindMyVolunteer.Data;
using FindMyVolunteer.Data.DataTransfer;
using FindMyVolunteer.Models;
using FindMyVolunteer.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FindMyVolunteer.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class SkillController(AppDbContext db, UserManager<AppUser> uMan): ControllerBase {
    [HttpGet("")]
    public async Task<IActionResult> Get() {
      return Ok(await db.Skills.Select(s => new {
        s.ID,
        s.Name,
      }).ToListAsync());
    }
    [HttpPost("")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post(CreateSkillDTO skill) {
      try {
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
      } catch(Exception e) { return StatusCode(500, e.Message); }
      return Ok();
    }
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Patch(int id, CreateSkillDTO skill) => await db.Skills
        .Where(s => s.ID == id)
        .ExecuteUpdateAsync(s => s
        .SetProperty(p => p.Name, skill.Name)
        .SetProperty(p => p.Description, skill.Description)) == 0 ? NotFound() : Ok();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id) => await db.Skills.Where(s => s.ID == id).ExecuteDeleteAsync() == 0 ? NotFound() : Ok();

    [HttpGet("event/{id}")]
    public async Task<IActionResult> GetRequiredSkills(int id) {
      return Ok(await db.RequiredSkills.Where(es => es.EventID == id).Select(es => new {
        es.SkillID,
        es.Skill.Name,
      }).ToListAsync());
    }
    [HttpPost("event/{id}")]
    [Authorize(Roles = "Organization")]
    public async Task<IActionResult> AddRequiredSkills(int id, List<int> skIds) {
      var skills = new List<RequiredSkills>();
      foreach(var skId in skIds) {
        skills.Add(new RequiredSkills {
          EventID = id,
          SkillID = skId,
        });
      }
      try {
        db.RequiredSkills.AddRange(skills);
        await db.SaveChangesAsync();
      } catch(Exception e) { return StatusCode(500, e.Message); }
      return Ok();
    }
    [HttpDelete("event/{id}")]
    [Authorize(Roles = "Organization")]
    public async Task<IActionResult> RemoveRequiredSkills(int id, List<int> skIds) {
      var skills = await db.RequiredSkills.Where(es => es.EventID == id && skIds.Contains(es.SkillID)).ToListAsync();
      try {
        db.RequiredSkills.RemoveRange(skills);
        await db.SaveChangesAsync();
      } catch(Exception e) { return StatusCode(500, e.Message); }
      return Ok();
    }
    [HttpPost("volunteer/{id}")]
    [Authorize("volunteer")]
    public async Task<IActionResult> AddVolunteerSkills(int id, List<int> skIds) {
      var skills = new List<VolunteerSkill>();
      for(int i = 0; i < 10; i++) skills.Add(new VolunteerSkill(id, skIds[i], i + 1));
      try {
        db.VolunteerSkills.AddRange(skills);
        await db.SaveChangesAsync();
      } catch(Exception e) { return StatusCode(500, e.Message); }
      return Ok();
    }
    [HttpDelete("volunteer/{id}")]
    [Authorize("volunteer")]
    public async Task<IActionResult> RemoveVolunteerSkills(int id, List<int> skIds) {
      var skills = await db.VolunteerSkills.Where(vs => vs.VolunteerId == id && skIds.Contains(vs.SkillId)).ToListAsync();
      try {
        db.VolunteerSkills.RemoveRange(skills);
        await db.SaveChangesAsync();
      } catch(Exception e) { return StatusCode(500, e.Message); }
      return Ok();
    }
  }
}
