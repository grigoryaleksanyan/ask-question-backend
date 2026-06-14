using AskQuestion.BLL.Email;
using AskQuestion.BLL.Helpers;
using AskQuestion.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AskQuestion.BLL.Tests.Helpers;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly DataContext _dataContext;
    private bool _disposed;

    protected RepositoryTestBase()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dataContext = new DataContext(options);
        DataContext = _dataContext;
        HtmlSanitizer = new HtmlSanitizerService();
        EmailSender = new FakeEmailSender();
        SmtpSettings = Options.Create(new SmtpSettings
        {
            BaseUrl = "http://localhost:5000",
            Host = "localhost",
            Port = 1025,
            FromEmail = "noreply@askquestion.local",
            FromName = "Ask Question",
        });
    }

    protected DataContext DataContext { get; }
    protected IHtmlSanitizerService HtmlSanitizer { get; }
    protected FakeEmailSender EmailSender { get; }
    protected IOptions<SmtpSettings> SmtpSettings { get; }

    public void Dispose()
    {
        if (!_disposed)
        {
            _dataContext.Dispose();
            _disposed = true;
        }
    }
}
