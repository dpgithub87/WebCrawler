using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebCrawler.Infrastructure.Clients.Interfaces;
using WebCrawler.Infrastructure.Config;
using WebCrawler.Infrastructure.Repository;
using WebCrawler.Infrastructure.Repository.Interfaces;
using WebCrawler.Infrastructure.UnitTests.Helpers;

namespace WebCrawler.Infrastructure.UnitTests.Repository;

public class WebPageRepositoryTests
{
    private readonly IFixture _fixture;

    public WebPageRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Theory, AutoMoqData]
    public async Task GetWebPageAsync_ShouldReturnCachedContent_WhenCacheHit(
        [Frozen] Mock<IWebPageDownloaderClient> webPageDownloaderClientMock,
        [Frozen] Mock<IDistributedCacheWrapper> cacheMock,
        [Frozen] Mock<ILogger<WebPageRepository>> loggerMock,
        [Frozen] IOptions<InfrastructureOptions> options,
        WebPageRepository repository)
    {
        // Arrange
        var targetUri = new Uri("http://example.com");
        var cancellationToken = CancellationToken.None;
        var cacheKey = $"WebPage_{targetUri.AbsoluteUri}";
        var cachedContent = "<html>Cached Content</html>";

        cacheMock.Setup(x => x.GetStringAsync(cacheKey, cancellationToken))
                 .ReturnsAsync(cachedContent);

        // Act
        var result = await repository.GetWebPageAsync(targetUri, cancellationToken);

        // Assert
        Assert.Equal(cachedContent, result);
        cacheMock.Verify(x => x.GetStringAsync(cacheKey, cancellationToken), Times.Once);
        webPageDownloaderClientMock.Verify(x => x.DownloadPageAsync(It.IsAny<Uri>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task GetWebPageAsync_ShouldReturnDownloadedContent_WhenCacheMiss(
        [Frozen] Mock<IWebPageDownloaderClient> webPageDownloaderClientMock,
        [Frozen] Mock<IDistributedCacheWrapper> cacheMock,
        [Frozen] Mock<ILogger<WebPageRepository>> loggerMock
       )
    {
        // Arrange
        var targetUri = new Uri("http://example.com");
        var cancellationToken = CancellationToken.None;
        var cacheKey = $"WebPage_{targetUri.AbsoluteUri}";
        var downloadedContent = "<html>Downloaded Content</html>";
        var infrastructureOptions = new InfrastructureOptions
        {
            CacheExpirySeconds = 60
        };

        var infrastructureSettings = Options.Create(infrastructureOptions);
        
        cacheMock.Setup(x => x.GetStringAsync(cacheKey, cancellationToken))
                 .ReturnsAsync((string)null);
        webPageDownloaderClientMock.Setup(x => x.DownloadPageAsync(targetUri))
                                   .ReturnsAsync(downloadedContent);

        // Act
        var repository = new WebPageRepository(webPageDownloaderClientMock.Object, cacheMock.Object, loggerMock.Object, infrastructureSettings);
        var result = await repository.GetWebPageAsync(targetUri, cancellationToken);

        // Assert
        Assert.Equal(downloadedContent, result);
        cacheMock.Verify(x => x.GetStringAsync(cacheKey, cancellationToken), Times.Once);
        webPageDownloaderClientMock.Verify(x => x.DownloadPageAsync(targetUri), Times.Once);
        cacheMock.Verify(x => x.SetStringAsync(cacheKey, downloadedContent, It.IsAny<DistributedCacheEntryOptions>(), cancellationToken), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task GetWebPageAsync_ShouldReturnNull_WhenDownloadFails(
        [Frozen] Mock<IWebPageDownloaderClient> webPageDownloaderClientMock,
        [Frozen] Mock<IDistributedCacheWrapper> cacheMock,
        [Frozen] Mock<ILogger<WebPageRepository>> loggerMock,
        [Frozen] IOptions<InfrastructureOptions> options,
        WebPageRepository repository)
    {
        // Arrange
        var targetUri = new Uri("http://example.com");
        var cancellationToken = CancellationToken.None;
        var cacheKey = $"WebPage_{targetUri.AbsoluteUri}";

        cacheMock.Setup(x => x.GetStringAsync(cacheKey, cancellationToken))
                 .ReturnsAsync((string)null);
        webPageDownloaderClientMock.Setup(x => x.DownloadPageAsync(targetUri))
                                   .ReturnsAsync((string)null);

        // Act
        var result = await repository.GetWebPageAsync(targetUri, cancellationToken);

        // Assert
        Assert.Null(result);
        cacheMock.Verify(x => x.GetStringAsync(cacheKey, cancellationToken), Times.Once);
        webPageDownloaderClientMock.Verify(x => x.DownloadPageAsync(targetUri), Times.Once);
        cacheMock.Verify(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), cancellationToken), Times.Never);
    }
}