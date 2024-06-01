using FindMyVolunteer.Data;
using FindMyVolunteer.Engines;
using FindMyVolunteer.Models.Identity;
using FindMyVolunteer.Seeders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddIdentity<AppUser, AppRole>()
  .AddEntityFrameworkStores<AppDbContext>()
  .AddDefaultTokenProviders();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.ConfigureApplicationCookie(options => {
  options.LoginPath = null;
  options.LogoutPath = null;
  options.Events.OnRedirectToLogin = context => {
    context.Response.StatusCode = 401;
    return Task.CompletedTask;
  };
  options.Events.OnRedirectToAccessDenied = context => {
    context.Response.StatusCode = 403;
    return Task.CompletedTask;
  };
});
builder.Services.AddAuthorization();
builder.Services.Configure<IdentityOptions>(options => {
  options.User.RequireUniqueEmail = true;
});
builder.Services.AddTransient<IEmailSender, EmailSender>();
#if DEBUG
builder.Services.AddSwaggerGen(c => {
  c.SwaggerDoc("v1", new() { Title = "FindMyVolunteer", Version = "v1" });
  //creating a cookie for the user's ID and assigning the value "1" to it
});
builder.Services.AddCors(options => {
  options.AddPolicy("AllowAll", builder => {
    builder.WithOrigins("https://scqwzkj1-44348.euw.devtunnels.ms/index.html");
  });
});
#endif
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDataProtection();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(30));
var app = builder.Build();
#if DEBUG
await app.SeedUsersAsyc();
await app.SeedEventsAsync();
app.UseSwagger();
app.UseSwaggerUI(c => {
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "FindMyVolunteer v1");
  c.RoutePrefix = string.Empty;
});
#endif
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.UseSession();
app.MapControllers();
app.MapRazorPages();
//var host = Dns.GetHostEntry(Dns.GetHostName());
//foreach (var ip in host.AddressList) {
//  //if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
//    Console.WriteLine($"https://{ip}:5001");
//    app.Urls.Add($"https://{ip}:5001");
//  //}
//}
app.Run();