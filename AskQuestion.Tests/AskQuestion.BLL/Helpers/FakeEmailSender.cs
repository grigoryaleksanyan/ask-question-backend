using AskQuestion.BLL.Email;

namespace AskQuestion.BLL.Tests.Helpers;

public sealed class FakeEmailSender : IEmailSender
{
    private readonly List<EmailMessage> _messages = new();

    public IReadOnlyList<EmailMessage> Messages => _messages.AsReadOnly();

    public Task EnqueueAsync(EmailMessage message)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }
}
