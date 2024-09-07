using System.Collections.Concurrent;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Executor.Config;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services;
using WebCrawler.Executor.Services.Interfaces;

namespace WebCrawler.Executor.UnitTests.Services;

public class UriProcessorServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICrawlResultsHandlerFactory> _crawlResultsHandlerFactoryMock = new();
    
    public UriProcessorServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Fact]
    public async Task ProcessUri_ShouldProcessSuccessfully()
    {
        // Arrange
        var uri = new Uri("http://example.com");
        var parentUri = new Uri("http://parent.com");
        var task = _fixture.Build<CrawlTask>()
                           .With(t => t.Uri, uri)
                           .With(t => t.ParentUri, parentUri)
                           .With(t => t.OutputFilePath, "output.csv")
                           .With(t => t.DepthLevel, 0)
                           .Create();
        var cancellationToken = CancellationToken.None;

        var webDownloaderMock = _fixture.Freeze<Mock<IWebPageDownloaderService>>();
        var uriExtractorMock = _fixture.Freeze<Mock<IUriExtractorService>>();
        var loggerMock = _fixture.Freeze<Mock<ILogger<UriProcessorService>>>();
        var crawlSettings = SetupCrawlSettings();

        webDownloaderMock.Setup(x => x.DownloadPage(uri, cancellationToken))
                         .ReturnsAsync("<html></html>");
        uriExtractorMock.Setup(x => x.ExtractValidUrls(It.IsAny<string>(), uri))
                        .Returns(new List<Uri> { new Uri("http://example.com/link1") });
        var crawlResultsHandlerMock = new Mock<ICrawlResultsHandler>();
        _crawlResultsHandlerFactoryMock.Setup(x => x.GetHandler("csv"))
                                      .Returns(crawlResultsHandlerMock.Object);

        var service = new UriProcessorService(
            webDownloaderMock.Object,
            uriExtractorMock.Object,
            new ConcurrentDictionary<Uri, bool>(),
            new BlockingCollection<CrawlTask>(),
            loggerMock.Object,
            _crawlResultsHandlerFactoryMock.Object,
            crawlSettings);

        // Act
        await service.ProcessUri(task, cancellationToken);

        // Assert
        Assert.Equal(CrawlTaskStatus.Completed, task.Status);
        webDownloaderMock.Verify(x => x.DownloadPage(uri, cancellationToken), Times.Once);
        uriExtractorMock.Verify(x => x.ExtractValidUrls(It.IsAny<string>(), uri), Times.Once);
        crawlResultsHandlerMock.Verify(x => x.WriteResults("output.csv", It.IsAny<List<CrawlResult>>()), Times.Once);
    }

    [Fact]
    public async Task ProcessUri_ShouldHandleDownloadFailure()
    {
        // Arrange
        var uri = new Uri("http://example.com");
        var parentUri = new Uri("http://parent.com");
        var task = _fixture.Build<CrawlTask>()
                           .With(t => t.Uri, uri)
                           .With(t => t.ParentUri, parentUri)
                           .With(t => t.OutputFilePath, "output.csv")
                           .With(t => t.DepthLevel, 0)
                           .Create();
        var cancellationToken = CancellationToken.None;

        var webDownloaderMock = _fixture.Freeze<Mock<IWebPageDownloaderService>>();
        var uriExtractorMock = _fixture.Freeze<Mock<IUriExtractorService>>();
        var loggerMock = _fixture.Freeze<Mock<ILogger<UriProcessorService>>>();
        var crawlSettings = SetupCrawlSettings();

        webDownloaderMock.Setup(x => x.DownloadPage(uri, cancellationToken))
                         .ReturnsAsync((string)null);

        var service = new UriProcessorService(
            webDownloaderMock.Object,
            uriExtractorMock.Object,
            new ConcurrentDictionary<Uri, bool>(),
            new BlockingCollection<CrawlTask>(),
            loggerMock.Object,
            _crawlResultsHandlerFactoryMock.Object,
            crawlSettings);

        // Act
        await service.ProcessUri(task, cancellationToken);

        // Assert
        Assert.Equal(CrawlTaskStatus.Failed, task.Status);
        webDownloaderMock.Verify(x => x.DownloadPage(uri, cancellationToken), Times.Once);
        uriExtractorMock.Verify(x => x.ExtractValidUrls(It.IsAny<string>(), uri), Times.Never);
    }
    
    private static IOptions<CrawlOptions> SetupCrawlSettings()
    {
        var crawlOptions = new CrawlOptions
        {
            InitialCrawlUris = "http://example.com",
            OutputFilePath = "output.csv",
            MaxDepth = 3,
            OutputFormat = "csv"
        };
        var crawlSettings = Options.Create(crawlOptions);
        return crawlSettings;
    }
}