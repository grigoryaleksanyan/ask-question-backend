namespace AskQuestion.BLL.Helpers;

public interface IHtmlSanitizerService
{
    /// <summary>
    /// Sanitizes the provided HTML string, removing dangerous tags and attributes.
    /// Returns empty string if input is null or whitespace.
    /// </summary>
    string Sanitize(string? html);
}
