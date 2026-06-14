using System.Net.Mail;

namespace AskQuestion.BLL.Email
{
    public class SmtpEmailClient(SmtpSettings settings) : IEmailClient
    {
        private readonly SmtpClient _client = new(settings.Host, settings.Port);

        public Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
            => _client.SendMailAsync(message, cancellationToken);

        public void Dispose() => _client.Dispose();
    }
}
