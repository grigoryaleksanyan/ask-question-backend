using AskQuestion.BLL.Email;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Email;

public class EmailSenderTests
{
    [Fact]
    public async Task EnqueueAsync_WritesMessageToChannel()
    {
        var sender = new EmailSender();
        var message = new EmailMessage
        {
            ToEmail = "to@test.com",
            ToName = "To",
            Subject = "Subject",
            HtmlBody = "Body",
        };

        await sender.EnqueueAsync(message);

        var read = await sender.GetReader().ReadAsync();
        read.Should().BeEquivalentTo(message);
    }
}
