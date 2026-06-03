namespace AskQuestion.BLL.Email
{
    public interface IEmailSender
    {
        Task EnqueueAsync(EmailMessage message);
    }
}
