namespace AskQuestion.BLL.Email
{
    public class SmtpEmailClientFactory(SmtpSettings settings) : IEmailClientFactory
    {
        public IEmailClient CreateClient() => new SmtpEmailClient(settings);
    }
}
