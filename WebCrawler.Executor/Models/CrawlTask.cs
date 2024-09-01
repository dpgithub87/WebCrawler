namespace WebCrawler.Executor.Models;

public class CrawlTask
{
    public Uri Uri { get; set; }
    public Uri ParentUri { get; set; }
    public CrawlTaskStatus Status { get; set; }
    public int Level { get; set; } // Depth level of the crawl

    public string OutputFilePath { get; set; }
    
    public CrawlTask(Uri uri, string outputFilePath, Uri parentUri = null, int level = 0)
    {
        Uri = uri;
        OutputFilePath = outputFilePath;
        ParentUri = parentUri;
        Status = CrawlTaskStatus.Pending;
        Level = level;
    }
}

public enum CrawlTaskStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}