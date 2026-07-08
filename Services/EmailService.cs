using System.Net;
using System.Net.Mail;
using HIVTraining_Vue.Server.Models;
using Microsoft.Extensions.Options;

namespace HIVTraining_Vue.Server.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var settings = _smtpSettings.IsDevelopment
                ? _smtpSettings.DevSettings
                : _smtpSettings.ProdSettings;

            using var smtpClient = new SmtpClient(settings.Host)
            {
                Port = settings.Port,
                EnableSsl = settings.EnableSSL,
                Credentials = new NetworkCredential(
                    settings.UserName,
                    settings.Password
                )
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(settings.UserName, settings.FromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}