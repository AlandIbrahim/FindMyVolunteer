using FindMyVolunteer.Models;
using FindMyVolunteer.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FindMyVolunteer.Data {
  public class AppDbContext(DbContextOptions<AppDbContext> options): IdentityDbContext<AppUser, AppRole, int>(options) {
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Volunteer> Volunteers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<ParticipationTicket> ParticipationTickets { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<VolunteerSkill> VolunteerSkills { get; set; }
    public DbSet<RequiredSkills> RequiredSkills { get; set; }
    public DbSet<EventRating> EventRatings { get; set; }
    //on model creating
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<ParticipationTicket>().HasKey(pt => new { pt.EventID, pt.VolunteerID });
      modelBuilder.Entity<Feedback>().HasKey(f => new { f.FromID, f.ToID });
      modelBuilder.Entity<EventRating>().HasKey(er => new { er.FromID, er.ToID });
      modelBuilder.Entity<Favorite>().HasKey(f => new { f.UserId, f.EventId });
      modelBuilder.Entity<VolunteerSkill>().HasKey(vs => new { vs.VolunteerId, vs.SkillId });
      modelBuilder.Entity<RequiredSkills>().HasKey(rs => new { rs.EventID, rs.SkillID });
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      base.OnConfiguring(optionsBuilder);
    }
  }
}
