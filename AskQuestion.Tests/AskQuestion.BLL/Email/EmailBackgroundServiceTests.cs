using AskQuestion.BLL.Email;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AskQuestion.BLL.Tests.Email
{
    public class EmailBackgroundServiceTests
    {
        private readonly EmailSender _emailSender = new();
        private readonly FakeEmailClientFactory _factory = new();
        private readonly IOptions<SmtpSettings> _settings = Options.Create(new SmtpSettings
        {
            Host = "localhost",
            Port = 1025,
            FromEmail = "noreply@test.com",
            FromName = "Test",
            BaseUrl = "http://localhost",
        });

        private EmailBackgroundService CreateService()
            => new(_emailSender, _factory, _settings, NullLogger<EmailBackgroundService>.Instance);

        [Fact]
        public async Task ExecuteAsync_SendsEnqueuedEmail()
        {
            var service = CreateService();
            await _emailSender.EnqueueAsync(new EmailMessage
            {
                ToEmail = "to@test.com",
                ToName = "To",
                Subject = "Subject",
                HtmlBody = "Body",
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            await service.StartAsync(cts.Token);
            await Task.Delay(100);
            await service.StopAsync(CancellationToken.None);

            _factory.Client.SentMessages.Should().ContainSingle();
            _factory.Client.SentMessages[0].To[0].Address.Should().Be("to@test.com");
        }

        [Fact]
        public async Task ExecuteAsync_ContinuesAfterSendFailure()
        {
            var service = CreateService();
            _factory.Client.ExceptionToThrow = new InvalidOperationException("SMTP failed");

            await _emailSender.EnqueueAsync(new EmailMessage
            {
                ToEmail = "fail@test.com",
                ToName = "Fail",
                Subject = "S",
                HtmlBody = "B",
            });
            await _emailSender.EnqueueAsync(new EmailMessage
            {
                ToEmail = "ok@test.com",
                ToName = "Ok",
                Subject = "S",
                HtmlBody = "B",
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            await service.StartAsync(cts.Token);
            await Task.Delay(100);
            await service.StopAsync(CancellationToken.None);

            _factory.Client.SentMessages.Should().ContainSingle();
            _factory.Client.SentMessages[0].To[0].Address.Should().Be("ok@test.com");
        }

        [Fact]
        public async Task ExecuteAsync_StopsOnCancellationToken()
        {
            var service = CreateService();

            using var cts = new CancellationTokenSource();
            await service.StartAsync(cts.Token);
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);

            _factory.Client.SentMessages.Should().BeEmpty();
        }
    }
}
