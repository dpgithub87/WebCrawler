using System.Diagnostics.CodeAnalysis;
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

        public bool IsValidUri(string link, Uri parentUri, out Uri? uri)
        {
            uri = null!;
            
            if (!SkipBookmarkAndInvalidLink(link)) return false;
           
            if (!SkipIfNotAbsoluteLink(link, parentUri, out uri)) return false;
            
            if (!SkipInValidSchemaAndIfNotParentDomain(parentUri, ref uri)) return false;
            
            if (!SkipParentUri(parentUri, uri)) return false;

            return true;
        }

        private bool SkipParentUri(Uri parentUri, Uri uri)
        {
            if (string.Equals(uri.Host, parentUri.Host, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(uri.PathAndQuery, parentUri.PathAndQuery, StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogInformation($"Not adding child link: {uri.ToString()} because it is the same as parent page");
                return false;
            }

            return true;
        }

        private bool SkipInValidSchemaAndIfNotParentDomain(Uri parentUri, [AllowNull] ref Uri uri)
        {
            if (uri!.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
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

            return true;
        }

        private bool SkipIfNotAbsoluteLink(string link, Uri parentUri, out Uri? uri)
        {
            if (link.StartsWith("/"))
            {
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
                if (!Uri.TryCreate(link, UriKind.Absolute, out uri))
                {
                    _logger.LogInformation($"Skipping invalid absolute link: {link}");
                    return false;
                }
            }

            return true;
        }

        private bool SkipBookmarkAndInvalidLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link) || link.StartsWith("#"))
            {
                _logger.LogInformation($"Skipping invalid or bookmark link: {link}");
                return false;
            }

            return true;
        }
    }

