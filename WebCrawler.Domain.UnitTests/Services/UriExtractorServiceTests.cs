using WebCrawler.Domain.UnitTests.Helpers;

namespace WebCrawler.Domain.UnitTests.Services;
using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Moq;
using WebCrawler.Domain.Helpers;
using WebCrawler.Domain.Services;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Domain.Validators;
using Xunit;

public class UriExtractorServiceTests
{
    private readonly IFixture _fixture;

    public UriExtractorServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Theory, AutoMoqData]
    public void ExtractValidUrls_ShouldReturnValidUrls(
        [Frozen] Mock<IHtmlParser> htmlParserMock,
        [Frozen] Mock<IUriValidator> uriValidatorMock,
        [Frozen] Mock<ILogger<UriExtractorService>> loggerMock,
        UriExtractorService service)
    {
        // Arrange
        var pageContent = "<html><a href='http://example.com'></a></html>";
        var parentUri = new Uri("http://parent.com");
        var links = new List<string> { "http://example.com" };
        var validUri = new Uri("http://example.com");

        htmlParserMock.Setup(x => x.GetLinks(pageContent)).Returns(links);
        uriValidatorMock.Setup(x => x.IsValidUri(It.IsAny<string>(), parentUri, out validUri)).Returns(true);

        // Act
        var result = service.ExtractValidUrls(pageContent, parentUri);

        // Assert
        Assert.Contains(validUri, result);
        htmlParserMock.Verify(x => x.GetLinks(pageContent), Times.Once);
        uriValidatorMock.Verify(x => x.IsValidUri(It.IsAny<string>(), parentUri, out validUri), Times.Once);
    }

    [Theory, AutoMoqData]
    public void ExtractValidUrls_ShouldSkipInvalidUrls(
        [Frozen] Mock<IHtmlParser> htmlParserMock,
        [Frozen] Mock<IUriValidator> uriValidatorMock,
        [Frozen] Mock<ILogger<UriExtractorService>> loggerMock,
        UriExtractorService service)
    {
        // Arrange
        var pageContent = "<html><a href='http://invalid.com'></a></html>";
        var parentUri = new Uri("http://parent.com");
        var links = new List<string> { "http://invalid.com" };
        Uri? validUri = null;

        htmlParserMock.Setup(x => x.GetLinks(pageContent)).Returns(links);
        uriValidatorMock.Setup(x => x.IsValidUri(It.IsAny<string>(), parentUri, out validUri)).Returns(false);

        // Act
        var result = service.ExtractValidUrls(pageContent, parentUri);

        // Assert
        Assert.Empty(result);
        htmlParserMock.Verify(x => x.GetLinks(pageContent), Times.Once);
        uriValidatorMock.Verify(x => x.IsValidUri(It.IsAny<string>(), parentUri, out validUri), Times.Once);
    }

    [Theory, AutoMoqData]
    public void ExtractValidUrls_ShouldLogDuplicateUrls(
        [Frozen] Mock<IHtmlParser> htmlParserMock,
        [Frozen] Mock<IUriValidator> uriValidatorMock,
        [Frozen] Mock<ILogger<UriExtractorService>> loggerMock,
        UriExtractorService service)
    {
        // Arrange
        var pageContent = "<html><a href='http://example.com'></a><a href='http://example.com'></a></html>";
        var parentUri = new Uri("http://parent.com");
        var links = new List<string> { "http://example.com", "http://example.com" };
        var validUri = new Uri("http://example.com");

        htmlParserMock.Setup(x => x.GetLinks(pageContent)).Returns(links);
        uriValidatorMock.Setup(x => x.IsValidUri(It.IsAny<string>(), parentUri, out validUri)).Returns(true);

        // Act
        var result = service.ExtractValidUrls(pageContent, parentUri);

        // Assert
        Assert.Single(result);
        htmlParserMock.Verify(x => x.GetLinks(pageContent), Times.Once);
        uriValidatorMock.Verify(x => x.IsValidUri(It.IsAny<string>(), parentUri, out validUri), Times.Exactly(2));
    }
}