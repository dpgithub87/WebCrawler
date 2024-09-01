using Microsoft.Extensions.Logging;
using WebCrawler.Domain.Helpers;

namespace WebCrawler.Domain.Validators;

 public class UriValidator : IUriValidator
    {
        private readonly ILogger<UriValidator> _logger;

        public UriValidator(ILogger<UriValidator> logger)
        {
            _logger = logger;
        }

        public bool IsValidUri(string link, Uri parentUri, out Uri uri)
        {
            uri = null!;
            
            // Skip bookmarks and invalid links
            if (string.IsNullOrWhiteSpace(link) || link.StartsWith("#"))
            {
                _logger.LogInformation($"Skipping invalid or bookmark link: {link}");
                return false;
            }

            // Convert relative links to absolute links
            if (link.StartsWith("/"))
            {
                // // Get the parent URI string without the last segment
                // var parentUriWithoutLastSegment = new Uri(parentUri.GetParentUriString());
                // uri = new Uri(parentUriWithoutLastSegment, link);
                // Convert relative links to absolute links
                var parentUriWithoutLastSegment = new Uri(parentUri.GetParentUriString());
                if (!Uri.TryCreate(parentUriWithoutLastSegment, link, out uri))
                {
                    _logger.LogInformation($"Skipping invalid relative link: {link}");
                    uri = null;
                    return false;
                }
            }
            else
            {
                // If the link is not a valid absolute URI, skip it
                if (!Uri.TryCreate(link, UriKind.Absolute, out uri))
                {
                    _logger.LogInformation($"Skipping invalid absolute link: {link}");
                    return false;
                }
            }

            // Check the scheme and domain
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                _logger.LogInformation($"Skipping link with unsupported scheme: {uri.Scheme}");
                uri = null!;
                return false;
            }

            if (uri.Host != parentUri.Host)
            {
                _logger.LogInformation($"Skipping link with different host: {uri.Host}");
                return false;
            }

            // Additional validation: Check if the URI is the same as the parent page
            if (string.Equals(uri.Host, parentUri.Host, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(uri.PathAndQuery, parentUri.PathAndQuery, StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogInformation($"Not adding child link: {uri.ToString()} because it is the same as parent page");
                return false;
            }

            return true;
        }
    }

