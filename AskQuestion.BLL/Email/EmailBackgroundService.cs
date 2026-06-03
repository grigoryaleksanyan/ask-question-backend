using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace AskQuestion.BLL.Email
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly EmailSender _emailSender;
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(
            IEmailSender emailSender,
            IOptions<SmtpSettings> smtpSettings,
            ILogger<EmailBackgroundService> logger)
        {
            _emailSender = (EmailSender)emailSender;
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailBackgroundService started");

            await foreach (var message in _emailSender.GetReader().ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                        Subject = message.Subject,
                        Body = message.HtmlBody,
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(new MailAddress(message.ToEmail, message.ToName));

                    using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port);
                    await smtpClient.SendMailAsync(mailMessage, stoppingToken);

                    _logger.LogInformation("Email sent to {Email}", message.ToEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email}", message.ToEmail);
                }
            }

            _logger.LogInformation("EmailBackgroundService stopped");
        }
    }
}
