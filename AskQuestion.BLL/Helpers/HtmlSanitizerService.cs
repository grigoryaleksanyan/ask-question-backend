using Ganss.Xss;

namespace AskQuestion.BLL.Helpers;

public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();

        // Clear all default allowed tags and set a strict whitelist
        _sanitizer.AllowedTags.Clear();
        string[] allowedTags = [
            "a", "b", "strong", "i", "em", "u",
            "ul", "ol", "li", "br", "p", "span", "div",
            "h1", "h2", "h3", "h4", "h5", "h6", "blockquote"
        ];
        foreach (string tag in allowedTags)
        {
            _sanitizer.AllowedTags.Add(tag);
        }

        // Clear all default allowed attributes and set a strict whitelist
        _sanitizer.AllowedAttributes.Clear();
        string[] allowedAttributes = ["href", "target", "class", "rel"];
        foreach (string attr in allowedAttributes)
        {
            _sanitizer.AllowedAttributes.Add(attr);
        }

        // Clear CSS properties (no inline styles allowed)
        _sanitizer.AllowedCssProperties.Clear();

        // Clear URI schemes and set strict whitelist
        _sanitizer.AllowedSchemes.Clear();
        string[] allowedSchemes = ["http", "https", "mailto", "tel"];
        foreach (string scheme in allowedSchemes)
        {
            _sanitizer.AllowedSchemes.Add(scheme);
        }

        // Only href is treated as a URI attribute
        _sanitizer.UriAttributes.Clear();
        _sanitizer.UriAttributes.Add("href");

        // Enforce rel="noopener noreferrer" on all target="_blank" links
        _sanitizer.PostProcessNode += (sender, e) =>
        {
            if (e.Node is AngleSharp.Html.Dom.IHtmlAnchorElement anchor
                && anchor.Target == "_blank")
            {
                string rel = anchor.Relation ?? "";
                List<string> relValues = rel
                    .Split([' '], StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                if (!relValues.Contains("noopener"))
                {
                    relValues.Add("noopener");
                }

                if (!relValues.Contains("noreferrer"))
                {
                    relValues.Add("noreferrer");
                }

                anchor.Relation = string.Join(" ", relValues);
            }
        };
    }

    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return html ?? string.Empty;
        }

        return _sanitizer.Sanitize(html);
    }
}
