using WorkTodo.Core.Constants;
using WorkTodo.Core.Validation;

namespace WorkTodo.Core.Entities;

public class TicketLink
{
    private static readonly string[] AllowedSchemes = [Uri.UriSchemeHttp, Uri.UriSchemeHttps];

    public TicketLink(string url, string? label = null)
    {
        Url = ValidatedUrl(url);
        Label = Guard.OptionalText(label, ValidationConstants.TicketLabelMaxLength, nameof(label));
    }

    public int Id { get; private set; }

    public string Url { get; private set; }

    public string? Label { get; private set; }

    private static string ValidatedUrl(string? url)
    {
        var trimmed = Guard.RequiredText(url, ValidationConstants.TicketUrlMaxLength, nameof(url));

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || !AllowedSchemes.Contains(uri.Scheme))
        {
            throw new ArgumentException("Value must be an absolute http or https URL.", nameof(url));
        }

        return trimmed;
    }
}
