using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Infrastructure.Repository.Interfaces;

public interface IWebContentRepository
{
    Task<WebContent?> GetWebPageAsync(Uri targetUri, CancellationToken cancellationToken);
}