namespace AskQuestion.BLL.Email
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
