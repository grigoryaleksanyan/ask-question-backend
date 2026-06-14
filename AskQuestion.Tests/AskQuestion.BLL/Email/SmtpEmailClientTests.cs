using AskQuestion.BLL.Email;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Email;

public class SmtpEmailClientTests
{
    [Fact]
    public void Constructor_CreatesClient()
    {
        var client = new SmtpEmailClient(new SmtpSettings { Host = "localhost", Port = 1025 });
        client.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var client = new SmtpEmailClient(new SmtpSettings { Host = "localhost", Port = 1025 });

        var act = () => client.Dispose();

        act.Should().NotThrow();
    }
}
