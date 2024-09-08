using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Domain.Services;

public class HtmlContentHandler : IWebContentHandler
{
    private readonly IUriExtractorService _uriExtractor;

    public HtmlContentHandler(IUriExtractorService uriExtractor)
    {
        _uriExtractor = uriExtractor;
    }

    public Task<(bool success, List<Uri>? links)> HandleContentAsync(WebContent htmlContent, Uri baseUri)
    {
        var links = _uriExtractor.ExtractValidUrls(htmlContent.Content!, baseUri);
        return Task.FromResult<(bool success, List<Uri>? links)>((true, links.ToList()));
    }
}