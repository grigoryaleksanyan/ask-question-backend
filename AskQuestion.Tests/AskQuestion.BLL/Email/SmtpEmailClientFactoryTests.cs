using AskQuestion.BLL.Email;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AskQuestion.BLL.Tests.Email;

public class SmtpEmailClientFactoryTests
{
    [Fact]
    public void CreateClient_ReturnsSmtpEmailClient()
    {
        var factory = new SmtpEmailClientFactory(Options.Create(new SmtpSettings
        {
            Host = "localhost",
            Port = 1025,
        }));

        var client = factory.CreateClient();

        client.Should().NotBeNull().And.BeOfType<SmtpEmailClient>();
    }
}
