namespace HIVTraining_Vue.Server.Models
{
    public class SmtpSettings
    {
        public bool IsDevelopment { get; set; }
        public SmtpAccountSettings DevSettings { get; set; } = new();
        public SmtpAccountSettings ProdSettings { get; set; } = new();
    }

    public class SmtpAccountSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromName { get; set; } = "HIV Training";
    }
}