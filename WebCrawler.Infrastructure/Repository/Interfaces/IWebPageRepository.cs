namespace WebCrawler.Infrastructure.Repository.Interfaces;

public interface IWebPageRepository
{
    Task<string?> GetWebPageAsync(Uri targetUri, CancellationToken cancellationToken);
}