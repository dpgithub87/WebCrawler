using System.Collections.Concurrent;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Hosting;
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
        [Frozen] Mock<IUriProcessorService> uriProcessorServiceMock)
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        Mock<IHostApplicationLifetime> hostApplicationLifetimeMock = new();
        hostApplicationLifetimeMock.SetupGet(x => x.ApplicationStopping).Returns(cancellationToken);

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
            hostApplicationLifetimeMock.Object,
            loggerMock.Object);

        // Act
        var executeTask = service.StartAsync(cancellationToken);

        // Simulate adding a task
        tasks.Add(new CrawlTask(new Uri("http://example.com"), "output/path"));

        // Allow some time for processing
        await Task.Delay(5000);

        // Cancel the service
        await cancellationTokenSource.CancelAsync();
        await executeTask;

        // Assert
        Assert.True(cancellationToken.IsCancellationRequested);
        Assert.Empty(tasks); // confirms the tasks were processed
        uriProcessorServiceMock.Verify(x => x.ProcessUri(It.IsAny<CrawlTask>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}