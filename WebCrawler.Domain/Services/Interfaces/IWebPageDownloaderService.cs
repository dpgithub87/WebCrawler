namespace WebCrawler.Domain.Services.Interfaces;

public interface IWebPageDownloaderService
{
    public Task<string?> DownloadPage(Uri targetUri);
}