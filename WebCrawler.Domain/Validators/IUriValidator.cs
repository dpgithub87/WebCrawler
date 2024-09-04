namespace WebCrawler.Domain.Validators;

public interface IUriValidator
{
    public bool IsValidUri(string link, Uri parentUri, out Uri? uri);
}