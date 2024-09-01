namespace WebCrawler.Domain.Helpers;

public static class UriExtensions
{
    public static string GetParentUriString(this Uri parentUri)
    {
        // Get the segments of the path
        var segments = parentUri.Segments;

        // If there are no segments or only one segment, return the base URI
        if (segments.Length <= 1)
        {
            return new Uri(parentUri.GetLeftPart(UriPartial.Authority)).ToString();
        }

        // Remove the last segment
        var parentPath = string.Join("", segments.Take(segments.Length - 1));

        // Build the new URI without the last segment
        var builder = new UriBuilder(parentUri)
        {
            Path = parentPath,
            Query = string.Empty // Remove the query if any
        };

        return builder.Uri.ToString();
    }
}