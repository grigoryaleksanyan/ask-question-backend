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
                var ex = ExceptionToThrow;
                ExceptionToThrow = null;
                throw ex;
            }

            SentMessages.Add(message);
            return Task.CompletedTask;
        }

        public void Dispose() { }
    }
}
