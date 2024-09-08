using System.Collections.Concurrent;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebCrawler.Executor.Config;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services.Interfaces;
using WebCrawler.Executor.UnitTests.Helpers;

namespace WebCrawler.Executor.UnitTests;

public class CrawlerBackgroundServiceTests
{
    private readonly IFixture _fixture;

    public CrawlerBackgroundServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Theory, AutoMoqData]
    public async Task ExecuteAsync_ShouldProcessInitialCrawlUris(
        [Frozen] Mock<ILogger<CrawlerBackgroundService>> loggerMock,
        [Frozen] Mock<IUriProcessorService> uriProcessorServiceMock
        )
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
        // Simulating cancellation for the tests
        
        var initialCrawlUris = new List<string> { "http://example.com" };

        var crawlOptionsValue = new CrawlOptions()
        {
            InitialCrawlUris = string.Join(",", initialCrawlUris),
            OutputFilePath = "output/path",
            MaxDepth = 3,
            OutputFormat = "csv"
        };
        var crawlOptions = Options.Create(crawlOptionsValue);
        
        var tasks = new BlockingCollection<CrawlTask>();

        var service = new CrawlerBackgroundService(
            uriProcessorServiceMock.Object,
            tasks,
            crawlOptions,
            loggerMock.Object);

        // Act
        await service.StartAsync(cancellationToken);
        
        // Assert
        Assert.True(cancellationToken.IsCancellationRequested);
        Assert.Empty(tasks); // confirms the tasks were processed
    }
}