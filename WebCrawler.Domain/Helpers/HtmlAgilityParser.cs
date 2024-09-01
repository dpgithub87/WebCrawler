using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace WebCrawler.Domain.Helpers;

public class HtmlAgilityParser : IHtmlParser
{
    private ILogger<HtmlAgilityParser> _logger;

    public HtmlAgilityParser(ILogger<HtmlAgilityParser> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> GetLinks(string htmlContent)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                return new List<string>();
            }

            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);

            var linkNodes = document.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes == null)
            {
                return new List<string>();
            }

            return linkNodes
                .Select(n => n.GetAttributeValue("href", string.Empty))
                .Where(href => !string.IsNullOrEmpty(href))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while parsing the HTML content in HtmlAgilityParser");
            throw;
        }
    }
}