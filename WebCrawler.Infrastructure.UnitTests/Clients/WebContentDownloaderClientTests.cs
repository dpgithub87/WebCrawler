using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WebCrawler.Infrastructure.Clients;
using WebCrawler.Infrastructure.Config;

namespace WebCrawler.Infrastructure.UnitTests.Clients;
public class WebContentDownloaderClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<WebContentDownloaderClient>> _loggerMock;
    private WebContentDownloaderClient _client;

    public WebContentDownloaderClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger<WebContentDownloaderClient>>();

        var infrastructureOptionsValue = new InfrastructureOptions { PollyRetryCount = 3 };
        var infrastructureOptions = Options.Create(infrastructureOptionsValue);

        _client = new WebContentDownloaderClient(_httpClient, _loggerMock.Object, infrastructureOptions);
    }

    [Fact]
    public async Task DownloadPageAsync_SuccessfulDownload_ReturnsContent()
    {
        // Arrange
        var targetUri = new Uri("http://example.com");
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html></html>")
        };
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.DownloadAsync(targetUri);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("<html></html>", result.Content);
    }

    [Fact]
    public async Task DownloadPageAsync_NonSuccessStatusCode_ReturnsNull()
    {
        // Arrange
        var targetUri = new Uri("http://example.com");
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);



        // Act
        var result = await _client.DownloadAsync(targetUri);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DownloadPageAsync_NonHtmlContentType_ReturnsContent()
    {
        // Arrange
        var targetUri = new Uri("http://example.com");
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Not HTML content")
        };
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.DownloadAsync(targetUri);

        // Assert
        Assert.NotNull(result);
    }
}