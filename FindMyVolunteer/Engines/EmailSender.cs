using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace FindMyVolunteer.Engines {
  public class EmailSender(IConfiguration config): IEmailSender {
    private readonly IConfiguration _config = config;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage) {
      var sender = new SmtpSender(() => new SmtpClient(_config["FluentEmail:Host"]) {
        EnableSsl = bool.Parse(_config["FluentEmail:EnableSsl"]),
        DeliveryMethod = SmtpDeliveryMethod.Network,
        Port = Convert.ToInt32(_config["FluentEmail:Port"]),
        UseDefaultCredentials = false,
        Credentials= new NetworkCredential(_config["FluentEmail:Username"], _config["FluentEmail:Password"])
      });
      Email.DefaultSender = sender;
      var emailMessage = await Email.From(_config["FluentEmail:From"])
        .To(email)
        .Subject(subject)
        .Body(htmlMessage,true)
        .SendAsync();
    }
  }
}
