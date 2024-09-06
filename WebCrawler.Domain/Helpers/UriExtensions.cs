namespace WebCrawler.Domain.Helpers;

public static class UriExtensions
{
    public static string GetParentUriString(this Uri parentUri)
    {
        var segments = parentUri.Segments;
        
        if (segments.Length <= 1)
        {
            return new Uri(parentUri.GetLeftPart(UriPartial.Authority)).ToString();
        }
        
        return RemoveLastSegmentAndQueryString(parentUri, segments);
    }

    private static string RemoveLastSegmentAndQueryString(Uri parentUri, string[] segments)
    {
        var parentPath = string.Join("", segments.Take(segments.Length - 1));
        
        var builder = new UriBuilder(parentUri)
        {
            Path = parentPath,
            Query = string.Empty 
        };

        return builder.Uri.ToString();
    }
}