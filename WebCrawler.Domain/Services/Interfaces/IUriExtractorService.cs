namespace WebCrawler.Domain.Services.Interfaces;

public interface IUriExtractorService
{
    public IEnumerable<Uri> ExtractValidUrls(string pageContent, Uri parentUri);
}