using AskQuestion.BLL.Email;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Email;

public class EmailTemplateBuilderTests
{
    [Fact]
    public void BuildNewQuestionNotification_ContainsRecipientAndQuestionUrl()
    {
        var result = EmailTemplateBuilder.BuildNewQuestionNotification(
            "to@test.com", "Speaker", "Question text", "http://localhost/q/1");

        result.ToEmail.Should().Be("to@test.com");
        result.ToName.Should().Be("Speaker");
        result.Subject.Should().Contain("вопрос");
        result.HtmlBody.Should().Contain("Question text");
        result.HtmlBody.Should().Contain("http://localhost/q/1");
    }

    [Fact]
    public void BuildSpeakerCredentials_ContainsPasswordAndLoginUrl()
    {
        var result = EmailTemplateBuilder.BuildSpeakerCredentials(
            "to@test.com", "Speaker", "Password123", "http://localhost/login");

        result.ToEmail.Should().Be("to@test.com");
        result.Subject.Should().Contain("учётная запись");
        result.HtmlBody.Should().Contain("Password123");
        result.HtmlBody.Should().Contain("http://localhost/login");
    }

    [Fact]
    public void BuildPasswordResetEmail_ContainsResetUrl()
    {
        var result = EmailTemplateBuilder.BuildPasswordResetEmail(
            "to@test.com", "User", "http://localhost/reset?token=abc");

        result.ToEmail.Should().Be("to@test.com");
        result.Subject.Should().Contain("Восстановление");
        result.HtmlBody.Should().Contain("http://localhost/reset?token=abc");
    }

    [Fact]
    public void BuildNewQuestionNotification_TruncatesLongText()
    {
        var longText = new string('A', 250);
        var result = EmailTemplateBuilder.BuildNewQuestionNotification(
            "to@test.com", "Speaker", longText, "http://localhost/q/1");

        result.HtmlBody.Should().Contain(new string('A', 200));
        result.HtmlBody.Should().Contain("…");
        result.HtmlBody.Should().NotContain(longText);
    }
}
