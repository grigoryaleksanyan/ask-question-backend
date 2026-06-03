using System.Threading.Channels;

namespace AskQuestion.BLL.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly Channel<EmailMessage> _channel =
            Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
            });

        public Task EnqueueAsync(EmailMessage message)
        {
            _channel.Writer.TryWrite(message);
            return Task.CompletedTask;
        }

        internal ChannelReader<EmailMessage> GetReader() => _channel.Reader;
    }
}
