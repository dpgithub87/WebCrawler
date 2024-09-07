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
using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Executor.UnitTests.Services;

public class UriProcessorServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICrawlResultsHandlerFactory> _crawlResultsHandlerFactoryMock = new();
    private readonly Mock<IDownloadedContentHandlerFactory> _downloadedContentHandlerFactoryMock = new();
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

        var webDownloaderMock = _fixture.Freeze<Mock<IWebContentDownloaderService>>();
        
        var loggerMock = _fixture.Freeze<Mock<ILogger<UriProcessorService>>>();
        var crawlSettings = SetupCrawlSettings();

        webDownloaderMock.Setup(x => x.DownloadContent(uri, cancellationToken))
                         .ReturnsAsync(new WebContent(){
                             Content = "<html></html>",
                             ContentType = "text/html"
                             });
        
        var crawlResultsHandlerMock = new Mock<ICrawlResultsHandler>();
        _crawlResultsHandlerFactoryMock.Setup(x => x.GetHandler("csv"))
                                      .Returns(crawlResultsHandlerMock.Object);

        var downloadedContentHandlerMock = _fixture.Freeze<Mock<IDownloadedContentHandler>>();
        downloadedContentHandlerMock.Setup(x => x.HandleContentAsync(It.IsAny<WebContent>(), uri))
                                    .ReturnsAsync((true, new List<Uri>()));
        
        _downloadedContentHandlerFactoryMock.Setup(x => x.CreateHandler("text/html"))
                                            .Returns(downloadedContentHandlerMock.Object);
        
        
        var service = new UriProcessorService(
            webDownloaderMock.Object,
            new ConcurrentDictionary<Uri, bool>(),
            new BlockingCollection<CrawlTask>(),
            loggerMock.Object,
            _crawlResultsHandlerFactoryMock.Object,
            _downloadedContentHandlerFactoryMock.Object,
            crawlSettings);

        // Act
        await service.ProcessUri(task, cancellationToken);

        // Assert
        Assert.Equal(CrawlTaskStatus.Completed, task.Status);
        webDownloaderMock.Verify(x => x.DownloadContent(uri, cancellationToken), Times.Once);
        downloadedContentHandlerMock.Verify(x => x.HandleContentAsync(It.IsAny<WebContent>(), uri), Times.Once);
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

        var webDownloaderMock = _fixture.Freeze<Mock<IWebContentDownloaderService>>();
        var uriExtractorMock = _fixture.Freeze<Mock<IUriExtractorService>>();
        var loggerMock = _fixture.Freeze<Mock<ILogger<UriProcessorService>>>();
        var crawlSettings = SetupCrawlSettings();

        webDownloaderMock.Setup(x => x.DownloadContent(uri, cancellationToken))
                         .ReturnsAsync((WebContent)null);
        var downloadedContentHandlerMock = new Mock<IDownloadedContentHandler>();
        _downloadedContentHandlerFactoryMock.Setup(x => x.CreateHandler("text/html"))
            .Returns(downloadedContentHandlerMock.Object);

        var service = new UriProcessorService(
            webDownloaderMock.Object,
            new ConcurrentDictionary<Uri, bool>(),
            new BlockingCollection<CrawlTask>(),
            loggerMock.Object,
            _crawlResultsHandlerFactoryMock.Object,
            _downloadedContentHandlerFactoryMock.Object, 
            crawlSettings);
        
        // Act
        await service.ProcessUri(task, cancellationToken);

        // Assert
        Assert.Equal(CrawlTaskStatus.Failed, task.Status);
        webDownloaderMock.Verify(x => x.DownloadContent(uri, cancellationToken), Times.Once);
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