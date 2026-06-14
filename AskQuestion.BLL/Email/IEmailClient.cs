using System.Net.Mail;

namespace AskQuestion.BLL.Email
{
    public interface IEmailClient : IDisposable
    {
        Task SendAsync(MailMessage message, CancellationToken cancellationToken = default);
    }
}
