using WebCrawler.Domain.Validators;

namespace WebCrawler.Domain.UnitTests.Validators;

using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class UriValidatorTests
{
    private readonly Mock<ILogger<UriValidator>> _mockLogger;
    private readonly IUriValidator _uriValidator;
    private Uri _parentUri;

    public UriValidatorTests()
    {
        _mockLogger = new Mock<ILogger<UriValidator>>();
        _uriValidator = new UriValidator(_mockLogger.Object);
        _parentUri = new Uri("https://example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("#bookmark")]
    public void IsValidUri_ShouldReturnFalse_ForInvalidOrBookmarkLinks(string link)
    {

        var result = _uriValidator.IsValidUri(link, _parentUri, out var uri);

        Assert.False(result);
        Assert.Null(uri);
    }

    [Fact]
    public void IsValidUri_ShouldReturnFalse_ForInvalidAbsoluteLink()
    {
        var link = "invalid://link";

        var result = _uriValidator.IsValidUri(link, _parentUri, out var uri);

        Assert.False(result);
        Assert.Null(uri);
    }

    [Fact]
    public void IsValidUri_ShouldReturnFalse_ForUnsupportedScheme()
    {
        var link = "ftp://example.com/resource";

        var result = _uriValidator.IsValidUri(link, _parentUri, out var uri);

        Assert.False(result);
    }

    [Fact]
    public void IsValidUri_ShouldReturnFalse_ForDifferentHost()
    {
        var link = "https://anotherdomain.com/resource";

        var result = _uriValidator.IsValidUri(link, _parentUri, out var uri);

        Assert.False(result);
    }

    [Fact]
    public void IsValidUri_ShouldReturnFalse_ForSameAsParentUri()
    {
        var link = "/resource";
        var parentUri = new Uri("https://example.com/resource");

        var result = _uriValidator.IsValidUri(link, parentUri, out var uri);

        Assert.False(result);
    }

    [Fact]
    public void IsValidUri_ShouldReturnTrue_ForValidRelativeLink()
    {
        var link = "/resource";

        var result = _uriValidator.IsValidUri(link, _parentUri, out var uri);

        Assert.True(result);
        Assert.NotNull(uri);
        Assert.Equal("https://example.com/resource", uri.ToString());
    }

    [Fact]
    public void IsValidUri_ShouldReturnTrue_ForValidAbsoluteLink()
    {
        var link = "https://example.com/resource";

        var result = _uriValidator.IsValidUri(link, _parentUri, out var uri);

        Assert.True(result);
        Assert.NotNull(uri);
        Assert.Equal(link, uri.ToString());
    }
    [Fact]
    public void IsValidUri_ShouldReturnTrue_ForValidAbsoluteLinkWithDifferentPath()
    {
        var link = "https://example.com/different-path";

        var result = _uriValidator.IsValidUri(link, _parentUri, out var uri);

        Assert.True(result);
        Assert.NotNull(uri);
        Assert.Equal(link, uri.ToString());
    }

}