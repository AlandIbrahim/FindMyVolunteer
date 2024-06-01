using FindMyVolunteer.Data;
using FindMyVolunteer.Data.Types;
using FindMyVolunteer.Models;
using FindMyVolunteer.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FindMyVolunteer.Seeders {
  public static class Seeder {
    public static async Task SeedEventsAsync(this WebApplication app) {
      var scope = app.Services.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      context.Database.EnsureCreated();
      if(context.Events.Any()) return;
      var orgs = await context.Organizations.Select(o => o.ID).ToListAsync();
      for(int i = 0; i < 100; i++) {
        var e = new Event {
          Title = $"Event {i}",
          Description = $"Description for event {i}",
          OrganizationID = orgs[Random.Shared.Next(1,orgs.Count)],
          EnrollmentDeadline = DateTime.Now.AddDays((Random.Shared.NextDouble()-0.3)*3),
          StartDate = DateTime.Now.AddDays((Random.Shared.NextDouble() - 0.15) * 5),
          Duration = TimeSpan.FromHours(Random.Shared.NextDouble() * 10),
          Location = $"Location {i}",
          City = (City)Random.Shared.Next(1, 26),
          MaxAttendees = (short)Random.Shared.Next(1, 10)
        };
        context.Events.Add(e);
      }
      await context.SaveChangesAsync();

    }
    public static async Task SeedUsersAsyc(this WebApplication app) {
      var scope = app.Services.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      context.Database.EnsureCreated();
      Console.WriteLine(context.Users.FirstOrDefault()?.UserName);
      if(context.Users.Any()) return;
      var uMan = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
      var rMan = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
      await rMan.CreateAsync(new AppRole() { Name = "Admin" });
      await rMan.CreateAsync(new AppRole() { Name = "Organization" });
      await rMan.CreateAsync(new AppRole() { Name = "Volunteer" });
      #region seed admin user
      var admin = new AppUser {
        UserName = "admin",
        Email = "aland.tci200223167223@spu.edu.iq",
      };
      await uMan.CreateAsync(admin, "Admin_12345");
      await uMan.AddToRoleAsync(admin, "Admin");
      #endregion
      #region seed organization users
      var org1 = new AppUser {
        UserName = "org1",
        Email = "fortest200223167223@gmail.com",
        IsOrganization = true,
        KYCVerified = true
      };
      var org1obj = new Organization {
        Name = "org1",
        Address = "org1 address",
        AppUser = org1,
        BusinessLicense = "org1 license",
        IsGovernmental = false,
      };
      await uMan.CreateAsync(org1, "For_12345");
      await uMan.AddToRoleAsync(org1, "Organization");
      context.Organizations.Add(org1obj);

      var org2 = new AppUser {
        UserName = "org2",
        Email = "randtest200223167223@gmail.com",
        IsOrganization = true,
        KYCVerified = false
      };
      var org2obj = new Organization {
        Name = "org2",
        Address = "org2 address",
        AppUser = org2,
        BusinessLicense = "org2 license",
        IsGovernmental = true,
      };
      await uMan.CreateAsync(org2, "For_12345");
      await uMan.AddToRoleAsync(org2, "Organization");
      context.Organizations.Add(org2obj);
      #endregion
      #region seed volunteer users
      var vol1 = new AppUser {
        UserName = "vol1",
        Email = "fortest200223167224@gmail.com",
        IsOrganization = false,
        KYCVerified = true
      };
      var vol1obj = new Volunteer {
        AppUser = vol1,
        FirstName = "first1",
        MiddleName = "middle1",
        LastName = "last1",
        Birthday = new DateOnly(2000, 1, 1),
        Gender = true,
        Languages = Languages.English | Languages.Kurdish | Languages.Arabic,
        City = City.Erbil,
        KYCIsPassport = true,
      };
      await uMan.CreateAsync(vol1, "For_12345");
      await uMan.AddToRoleAsync(vol1, "Volunteer");
      context.Volunteers.Add(vol1obj);
      for(int i = 0; i < 10; i++) {
        var vol = new AppUser {
          UserName = $"vol{i + 2}",
          Email = $"fortest{Random.Shared.Next(9999)}231672{i}_@gmail.com",
          IsOrganization = false,
        };
        var volobj = new Volunteer {
          AppUser = vol,
          FirstName = _namePool[Random.Shared.Next(_namePool.Length)],
          MiddleName = Random.Shared.Next(0, 2) == 0 ? _namePool[Random.Shared.Next(_namePool.Length)] : null,
          LastName = _namePool[Random.Shared.Next(_namePool.Length)],
          Birthday = new DateOnly(Random.Shared.Next(1980, 2005), Random.Shared.Next(1, 13), Random.Shared.Next(1, 29)),
          Gender = Random.Shared.Next(0, 2) == 0,
          Languages = (Languages)Random.Shared.Next(1, 64),
          City = (City)Random.Shared.Next(1, 26),
          KYCIsPassport = Random.Shared.Next(0, 2) == 0,
        };
        await uMan.CreateAsync(vol, "For_12345");
        await uMan.AddToRoleAsync(vol, "Volunteer");
        context.Volunteers.Add(volobj);
      }
      #endregion
      await context.SaveChangesAsync();
    }

    private static readonly string[] _namePool = [
    "Ava", "Olivia", "Amelia", "Isla", "Emily", "Mia", "Aria", "Sophia", "Grace", "Lily",
    "Isabella", "Sophie", "Ivy", "Willow", "Harper", "Zoe", "Ella", "Chloe", "Aurora", "Lucy",
    "Ruby", "Scarlett", "Evelyn", "Alice", "Matilda", "Daisy", "Luna", "Freya", "Maya", "Nora",
    "Elsie", "Sofia", "Rosie", "Harriet", "Pippa", "Penelope", "Mila", "Violet", "Ayla", "Eva",
    "Emilia", "Elizabeth", "Layla", "Lottie", "Mabel", "Molly", "Eliza", "Sienna", "Ellie", "Ariana",
    "Hazel", "Claire", "Astrid", "Athena", "Nina", "Ariella", "Aurelia", "Cora", "Daphne", "Diana",
    "Fiona", "Gemma", "Iris", "Juno", "Lyra", "Maeve", "Nova", "Olive", "Phoebe", "Rhea",
    "Serena", "Thea", "Thalia", "Vera", "Willa", "Zara", "Adeline", "Anastasia", "Arabella", "Beatrix",
    "Cecilia", "Clementine", "Delilah", "Edith", "Eloise", "Estelle", "Evangeline", "Florence", "Genevieve", "Guinevere",
    "Imogen", "Isolde", "Josephine", "Juliet", "Lavinia", "Lilith", "Magnolia", "Margot", "Marigold", "Mirabel",
    "Ophelia", "Persephone", "Rosalind", "Rowena", "Sylvie", "Tallulah", "Valentina", "Verity", "Vivienne", "Winifred",
    "Zelda", "Adelaide", "Agnes", "Antonia", "August" ];
  }
}
