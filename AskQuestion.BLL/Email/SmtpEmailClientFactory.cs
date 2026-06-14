using Microsoft.Extensions.Options;

namespace AskQuestion.BLL.Email
{
    public class SmtpEmailClientFactory(IOptions<SmtpSettings> options) : IEmailClientFactory
    {
        public IEmailClient CreateClient() => new SmtpEmailClient(options.Value);
    }
}
