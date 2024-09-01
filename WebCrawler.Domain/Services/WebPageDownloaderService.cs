using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Infrastructure.Clients.Interfaces;

namespace WebCrawler.Domain.Services;

public class WebPageDownloaderService : IWebPageDownloaderService
{
    private readonly IWebPageDownloaderClient _webPageDownloaderClient;
    public WebPageDownloaderService(IWebPageDownloaderClient webPageDownloaderClient)
    {
        _webPageDownloaderClient = webPageDownloaderClient;
    }
   public async Task<string?> DownloadPage(Uri targetUri)
   {
       if ((targetUri.Scheme.ToLower() != "http") && (targetUri.Scheme.ToLower() != "https"))
       {
           return null;
       }
       
       return await _webPageDownloaderClient.DownloadPageAsync(targetUri);
   }
}