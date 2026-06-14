using System.Threading.Channels;
using AskQuestion.BLL.Email;

namespace AskQuestion.BLL.Tests.Helpers;

public sealed class FakeEmailSender : IEmailSender
{
    private readonly List<EmailMessage> _messages = new();
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });

    public IReadOnlyList<EmailMessage> Messages => _messages.AsReadOnly();

    public Task EnqueueAsync(EmailMessage message)
    {
        _messages.Add(message);
        _channel.Writer.TryWrite(message);
        return Task.CompletedTask;
    }

    public ChannelReader<EmailMessage> GetReader() => _channel.Reader;
}
