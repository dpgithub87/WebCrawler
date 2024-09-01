namespace WebCrawler.Domain.Helpers;

public interface IHtmlParser
{
    IEnumerable<string> GetLinks(string htmlContent);
}