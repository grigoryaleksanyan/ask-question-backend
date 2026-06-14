using System.Threading.Channels;

namespace AskQuestion.BLL.Email
{
    public interface IEmailSender
    {
        Task EnqueueAsync(EmailMessage message);
        ChannelReader<EmailMessage> GetReader();
    }
}
