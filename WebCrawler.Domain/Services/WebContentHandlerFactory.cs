using Microsoft.Extensions.DependencyInjection;
using WebCrawler.Domain.Services.Interfaces;

namespace WebCrawler.Domain.Services;

public class WebContentHandlerFactory : IWebContentHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    private const string HtmlContentType = "text/html";
    
    public WebContentHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IWebContentHandler CreateHandler(string contentType)
    {
        if (contentType.Contains(HtmlContentType))
        {
            return _serviceProvider.GetRequiredService<IWebContentHandler>();
        }

        // Add more handlers for different content types if needed
        throw new NotSupportedException($"Content type {contentType} is not supported.");
    }
}