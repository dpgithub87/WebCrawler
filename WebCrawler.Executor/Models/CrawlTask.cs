namespace WebCrawler.Executor.Models;

public class CrawlTask
{
    public Uri Uri { get; set; }
    public Uri ParentUri { get; set; }
    public CrawlTaskStatus Status { get; set; }
    public int DepthLevel { get; set; }

    public string OutputFilePath { get; set; }
    
    public CrawlTask(Uri uri, string outputFilePath, Uri parentUri = null, int depthLevel = 0)
    {
        Uri = uri;
        OutputFilePath = outputFilePath;
        ParentUri = parentUri;
        Status = CrawlTaskStatus.Pending;
        DepthLevel = depthLevel;
    }
}

public enum CrawlTaskStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}