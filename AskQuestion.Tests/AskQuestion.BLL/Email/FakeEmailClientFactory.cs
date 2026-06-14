using AskQuestion.BLL.Email;

namespace AskQuestion.BLL.Tests.Email;

public class FakeEmailClientFactory : IEmailClientFactory
{
    public FakeEmailClient Client { get; } = new();

    public IEmailClient CreateClient() => Client;
}
