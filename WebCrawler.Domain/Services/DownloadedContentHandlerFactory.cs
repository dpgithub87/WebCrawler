using Microsoft.Extensions.DependencyInjection;
using WebCrawler.Domain.Services.Interfaces;

namespace WebCrawler.Domain.Services;

public class DownloadedContentHandlerFactory : IDownloadedContentHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    private const string HtmlContentType = "text/html";
    
    public DownloadedContentHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDownloadedContentHandler CreateHandler(string contentType)
    {
        if (contentType.Contains(HtmlContentType))
        {
            return _serviceProvider.GetRequiredService<IDownloadedContentHandler>();
        }

        // Add more handlers for different content types if needed
        throw new NotSupportedException($"Content type {contentType} is not supported.");
    }
}