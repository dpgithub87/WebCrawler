using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Moq;
using WebCrawler.Domain.Services;
using WebCrawler.Domain.UnitTests.Helpers;
using WebCrawler.Infrastructure.Repository.Interfaces;

namespace WebCrawler.Domain.UnitTests.Services;

public class WebPageDownloaderServiceTests
{
    private readonly IFixture _fixture;

    public WebPageDownloaderServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Theory, AutoMoqData]
    public async Task DownloadPage_ShouldReturnContent_WhenUriIsValid(
        [Frozen] Mock<IWebPageRepository> webPageRepositoryMock,
        [Frozen] Mock<ILogger<WebPageDownloaderService>> loggerMock,
        WebPageDownloaderService service)
    {
        // Arrange
        var targetUri = new Uri("http://example.com");
        var cancellationToken = CancellationToken.None;
        var expectedContent = "<html></html>";

        webPageRepositoryMock.Setup(x => x.GetWebPageAsync(targetUri, cancellationToken))
                             .ReturnsAsync(expectedContent);

        // Act
        var result = await service.DownloadPage(targetUri, cancellationToken);

        // Assert
        Assert.Equal(expectedContent, result);
        webPageRepositoryMock.Verify(x => x.GetWebPageAsync(targetUri, cancellationToken), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task DownloadPage_ShouldReturnNull_WhenUriIsInvalidScheme(
        [Frozen] Mock<IWebPageRepository> webPageRepositoryMock,
        [Frozen] Mock<ILogger<WebPageDownloaderService>> loggerMock,
        WebPageDownloaderService service)
    {
        // Arrange
        var targetUri = new Uri("ftp://example.com");
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await service.DownloadPage(targetUri, cancellationToken);

        // Assert
        Assert.Null(result);
        webPageRepositoryMock.Verify(x => x.GetWebPageAsync(It.IsAny<Uri>(), cancellationToken), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task DownloadPage_ShouldLogError_WhenExceptionIsThrown(
        [Frozen] Mock<IWebPageRepository> webPageRepositoryMock,
        [Frozen] Mock<ILogger<WebPageDownloaderService>> loggerMock,
        WebPageDownloaderService service)
    {
        // Arrange
        var targetUri = new Uri("http://example.com");
        var cancellationToken = CancellationToken.None;
        var exception = new Exception("Network error");

        webPageRepositoryMock.Setup(x => x.GetWebPageAsync(targetUri, cancellationToken))
                             .ThrowsAsync(exception);

        // Act
        var result = await service.DownloadPage(targetUri, cancellationToken);

        // Assert
        Assert.Null(result);
        webPageRepositoryMock.Verify(x => x.GetWebPageAsync(targetUri, cancellationToken), Times.Once);
    }
}