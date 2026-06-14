using System.Net.Mail;
using AskQuestion.BLL.Email;

namespace AskQuestion.BLL.Tests.Email;

public class FakeEmailClient : IEmailClient
{
    private readonly object _lock = new();
    private readonly List<TaskCompletionSource> _waiters = new();

    public List<MailMessage> SentMessages { get; } = new();
    public Exception? ExceptionToThrow { get; set; }

    public Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow != null)
        {
            var ex = ExceptionToThrow;
            ExceptionToThrow = null;
            throw ex;
        }

        SentMessages.Add(message);
        CompleteWaiters();
        return Task.CompletedTask;
    }

    public Task WaitForMessageCountAsync(int count, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource tcs;
        lock (_lock)
        {
            if (SentMessages.Count >= count)
            {
                return Task.CompletedTask;
            }

            while (_waiters.Count < count)
            {
                _waiters.Add(new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
            }

            tcs = _waiters[count - 1];
        }

        return tcs.Task.WaitAsync(cancellationToken);
    }

    private void CompleteWaiters()
    {
        lock (_lock)
        {
            for (int i = 0; i < SentMessages.Count && i < _waiters.Count; i++)
            {
                _waiters[i].TrySetResult();
            }
        }
    }

    public void Dispose() { }
}
