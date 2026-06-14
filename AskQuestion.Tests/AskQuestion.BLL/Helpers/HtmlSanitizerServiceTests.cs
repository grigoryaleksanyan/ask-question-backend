using AskQuestion.BLL.Helpers;
using FluentAssertions;

namespace AskQuestion.BLL.Tests.Helpers;

public class HtmlSanitizerServiceTests
{
    private readonly HtmlSanitizerService _sanitizer = new();

    [Fact]
    public void Sanitize_ReturnsNull_ForNullInput()
    {
        _sanitizer.Sanitize(null).Should().BeNull();
    }

    [Fact]
    public void Sanitize_ReturnsEmptyString_ForWhitespaceInput()
    {
        _sanitizer.Sanitize("   ").Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_RemovesScriptTag()
    {
        var result = _sanitizer.Sanitize("<script>alert('xss')</script>Hello");

        result.Should().NotContain("<script");
        result.Should().Contain("Hello");
    }

    [Fact]
    public void Sanitize_KeepsAllowedTags()
    {
        var result = _sanitizer.Sanitize("<p><b>Bold</b> and <a href=\"http://example.com\">link</a></p>");

        result.Should().Contain("<p>");
        result.Should().Contain("<b>");
        result.Should().Contain("<a href=\"http://example.com\">");
    }

    [Fact]
    public void Sanitize_RemovesDisallowedAttributes()
    {
        var result = _sanitizer.Sanitize("<a href=\"http://example.com\" onclick=\"evil()\">link</a>");

        result.Should().Contain("<a href=\"http://example.com\">");
        result.Should().NotContain("onclick");
    }

    [Fact]
    public void Sanitize_AddsRelNoopenerNoreferrer_ForTargetBlankLinks()
    {
        var result = _sanitizer.Sanitize("<a href=\"http://example.com\" target=\"_blank\">link</a>");

        result.Should().Contain("rel=\"noopener noreferrer\"");
    }
}
