using System.Net.Mail;
using AskQuestion.BLL.Email;

namespace AskQuestion.BLL.Tests.Email
{
    public class FakeEmailClient : IEmailClient
    {
        public List<MailMessage> SentMessages { get; } = new();
        public Exception? ExceptionToThrow { get; set; }

        public Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            SentMessages.Add(message);
            return Task.CompletedTask;
        }

        public void Dispose() { }
    }
}
