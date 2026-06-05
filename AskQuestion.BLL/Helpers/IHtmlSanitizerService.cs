namespace AskQuestion.BLL.Helpers;

public interface IHtmlSanitizerService
{
    /// <summary>
    /// Sanitizes the provided HTML string, removing dangerous tags and attributes.
    /// Returns null if input is null, empty string if input is whitespace only.
    /// </summary>
    string? Sanitize(string? html);
}
