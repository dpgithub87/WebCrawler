using Microsoft.Extensions.Logging;
using WebCrawler.Domain.Helpers;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Domain.Validators;

namespace WebCrawler.Domain.Services;

public class UriExtractorService : IUriExtractorService
{
    private readonly IHtmlParser _htmlParser;
    private readonly IUriValidator _uriValidator;
    private readonly ILogger<UriExtractorService> _logger;
    
    public UriExtractorService(IHtmlParser htmlParser, IUriValidator uriValidator, ILogger<UriExtractorService> logger)
    {
        _htmlParser = htmlParser;
        _uriValidator = uriValidator;
        _logger = logger;
    }
    public IEnumerable<Uri> ExtractValidUrls(string pageContent, Uri parentUri)
    {
        var urls = new HashSet<Uri>();
        //The HashSet provides efficient O(1) time complexity for lookups and insertions.
        
        var links = _htmlParser.GetLinks(pageContent);
        foreach (var link in links)
        {
            if (_uriValidator.IsValidUri(link, parentUri, out var uri) && !urls.Contains(uri!))
            {
                var addIfNotDuplicate = urls.Add(uri!);
                if (!addIfNotDuplicate)
                    _logger.LogInformation($"Skipping duplicate link: {uri!.ToString()}");
            }
        }
        
        return urls;
    }
    
}