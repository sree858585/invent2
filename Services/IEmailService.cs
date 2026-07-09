namespace HIVTraining_Vue.Server.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string? ccEmail = null);
    }
}