using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Domain.Services.Interfaces;

public interface IDownloadedContentHandler
{
    Task<(bool success, List<Uri>? links)> HandleContentAsync(WebContent content, Uri baseUri);
}