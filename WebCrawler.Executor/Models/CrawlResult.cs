using System.Text.Json.Serialization;

namespace WebCrawler.Executor.Models;

public class CrawlResult
{
    [JsonPropertyName("uri")]
    public Uri Uri { get; set; }
    
    [JsonPropertyName("parentUri")]
    public Uri? ParentUri { get; set; }
    
    [JsonPropertyName("links")]
    public List<Uri> Links { get; set; }
    
    [JsonPropertyName("crawlTime")]
    public TimeSpan CrawlTime { get; set; }
   
    [JsonPropertyName("level")]
    public int DepthLevel { get; set; } 

    public CrawlResult(Uri uri, Uri? parentUri)
    {
        Uri = uri;
        ParentUri = parentUri;
        Links = new List<Uri>();
    }
    
}