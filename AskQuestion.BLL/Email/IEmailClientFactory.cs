namespace AskQuestion.BLL.Email
{
    public interface IEmailClientFactory
    {
        IEmailClient CreateClient();
    }
}
