namespace Songhay.Social.Models;

public class DeliciousLink
{
    public Uri? Href { get; init; }
    public DateTime? AddDate { get; init; }
    public bool? IsPrivate { get; init; }
    public string? Tags { get; init; }
    public string? Title { get; init; }
    public string? DD { get; set; }
}
