using CsvHelper.Configuration;
using WebCrawler.Executor.Models;

namespace WebCrawler.Executor.Services.CrawlResultsHandler;

public class CsvCrawlResultMap : ClassMap<CrawlResult>
{
    public CsvCrawlResultMap()
    {
        Map(m => m.Uri).Name("Uri");
        Map(m => m.ParentUri).Name("ParentUri");
        Map(m => m.Links).Convert(row => string.Join(",", row.Value.Links));
        Map(m => m.CrawlTime).Name("CrawlTime");
        Map(m => m.DepthLevel).Name("DepthLevel");
    }
}