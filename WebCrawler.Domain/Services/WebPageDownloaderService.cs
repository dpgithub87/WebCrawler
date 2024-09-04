using Microsoft.Extensions.Logging;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Infrastructure.Clients.Interfaces;
using WebCrawler.Infrastructure.Repository.Interfaces;

namespace WebCrawler.Domain.Services;

public class WebPageDownloaderService : IWebPageDownloaderService
{
    private readonly IWebPageRepository _webPageRepository;
    private readonly ILogger<WebPageDownloaderService> _logger;

    public WebPageDownloaderService(IWebPageRepository webPageRepository, ILogger<WebPageDownloaderService> logger)
    {
        _webPageRepository = webPageRepository;
        _logger = logger;
    }
   public async Task<string?> DownloadPage(Uri targetUri, CancellationToken cancellationToken)
   {
       try
       {
           if ((targetUri.Scheme.ToLower() != "http") && (targetUri.Scheme.ToLower() != "https"))
           {
               return null;
           }

           return await _webPageRepository.GetWebPageAsync(targetUri, cancellationToken);
       }
       catch (Exception e)
       {
           _logger.LogError(e, "Error downloading page");
           return null;
       }
   }
}