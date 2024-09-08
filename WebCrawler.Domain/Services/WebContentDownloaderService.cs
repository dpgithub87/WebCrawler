using Microsoft.Extensions.Logging;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Infrastructure.Clients.Interfaces;
using WebCrawler.Infrastructure.Models;
using WebCrawler.Infrastructure.Repository.Interfaces;

namespace WebCrawler.Domain.Services;

public class WebContentDownloaderService : IWebContentDownloaderService
{
    private readonly IWebContentRepository _webContentRepository;
    private readonly ILogger<WebContentDownloaderService> _logger;

    public WebContentDownloaderService(IWebContentRepository webPageRepository, ILogger<WebContentDownloaderService> logger)
    {
        _webContentRepository = webPageRepository;
        _logger = logger;
    }
    public async Task<WebContent?> DownloadContent(Uri targetUri, CancellationToken cancellationToken)
    {
        try
        {
            if ((targetUri.Scheme.ToLower() != "http") && (targetUri.Scheme.ToLower() != "https"))
            {
                return null;
            }

            return await _webContentRepository.GetWebPageAsync(targetUri, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error downloading page");
            return null;
        }
    }
}