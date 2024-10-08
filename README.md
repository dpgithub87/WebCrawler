# Simple Web Crawler

A lightweight and efficient web crawler that recursively crawls through a website, retrieves sub-links, and continues to crawl those pages, all while staying within the domain of the parent site.

## Features

- Crawls through websites, gathering sub-links.
- Only crawls links within the parent domain.
- Extracts URIs, validate and standardize them. Avoids circular references between pages.
- Supports only HTML content downloads. 
- Supports output in various formats such as JSON.

## System Design

### Core Components

This project follows the Clean code architecture where we have following components:
  - Executor (API / Interface layer)
    - **Crawler Executor**: This is the main component responsible for orchestrating the crawling process.
    - **Background Service**: Manages concurrent crawling using parallel tasks.
  - Domain (Core business logic) - independent of any I/O operations.
    - **UriExtractor**: Extracts all valid URLs from the HTML content. It uses Html Agility Parser to extract Uris.
    - **UriValidator**: Validate the fetched Uris; check if it is having the same parent domain. 
  - Infrastructure - Contains the I/O operations - HttpClient
    - **WebPageDownloader**: Fetches the HTML content of each page.
    - **WebPageRepository**: Caches the webpage downloaded for a specified amount of time. Uses IDistributed Redis Cache.

### Design Patterns
- Factory Pattern:
  The Factory Pattern is used to dynamically create handlers for downloaded content based on response headers. Although it currently supports only HTML content, it's designed for extensibility to accommodate other formats in the future. Additionally, the pattern is applied to generate CrawlResultHandlers based on the desired output format.
- Repository Pattern:
  Utilized to separate the client layer from the underlying cache implementation, providing abstraction and flexibility.

### Fault Tolerance

- **Polly Library**: Configured with retries (3 times by default) using exponential backoff to handle transient errors.

### Caching

- **Distributed Caching**: Leverage Redis or similar services to cache upstream API responses.

### Data structures
- Thread safe data structures are used to store the background task details and to store the list of processed Uris facilitating the concurrent execution.

![crawler_system_architecture](crawler_system_architecture.png)

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Docker Desktop 4.22
- Docker Engine - v24^
- OS: Windows 10 / Linux / Mac

## Usage

### 1. Clone or Download the Repository

You can either download this repository as a [zip file](https://github.com/dpgithub87/WebCrawler/archive/refs/heads/main.zip) or clone it via git.
```bash
git clone https://github.com/dpgithub87/WebCrawler.git
```
### 2. Run the Project Locally

To run the crawler, navigate to the root directory (`WebCrawler/`) and execute the below command in your terminal.
```bash
cd WebCrawler
dotnet run --project WebCrawler.Executor/WebCrawler.Executor.csproj --url "https://www.google.com" --maxdepth 1 --format "json"
```
**Output**: The results will be saved in the `Output` folder located in `WebCrawler/WebCrawler.Executor/Output`.

### 3. Running the Crawler in Docker (Optional)

To run the crawler within Docker, follow these steps:

1. Build the Docker image.
```bash
docker build -t webcrawler-executor:v1.0 .
```
2. Run the Docker container.
```bash
docker run --name web-crawler-container -it -e ASPNETCORE_ENVIRONMENT=Development webcrawler-executor:v1.0 --url "https://www.google.com" --maxdepth 1 --format "json"
```
3. The Crawler app will automatically shut down if no new URIs are available to crawl within a 10-second window. After that, you can retrieve the output from the Docker container to your local machine.
```bash
docker cp web-crawler-container:/app/Output .
```
**Crawler Arguments Explanation**:
- **URL to Crawl**: url - A comma separated list of initial URLs to crawl.
- **Page Depth Limit**: maxdepth - You can impose a limit on the level of depth to crawl in BFS(breadh first search) manner.
- **Output Format**: format - Format of the output export file, currently supports JSON/CSV.

**Note**: The first run may take additional time as Docker downloads the base ASP.NET 8.0 and SDK images. Subsequent runs will be faster due to caching.

## Testing
The test projects include unit tests and integration tests. Integration tests will query the "https://www.google.com" website for data.
The unit tests run with mocked data.
Execute the below "test" command by navigating to root folder `WebCrawler/` in CLI.
```sh
dotnet test
```
The test results will be displayed in the CLI.
```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    17, Skipped:     0, Total:    17, Duration: 215 ms - WebCrawler.Domain.UnitTests.dll (net8.0)

Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 4 s - WebCrawler.Executor.UnitTests.dll (net8.0)

Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: < 1 ms - WebCrawler.Executor.Integration.Tests.dll (net8.0)

Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 14 s - WebCrawler.Infrastructure.UnitTests.dll (net8.0)
```

## Productionize the Application
### Microservice Architecture

- Extract the WebDownloader and UriExtractor as independent microservices to enhance horizontal scaling and efficiency.

### Parallel Processing at Kubernetes layer

- Implement the crawler-executor either as a Cron job or as a deployment with multiple pods to monitor a queue for new sites, allowing for parallel crawling.

### Error Handling

- Enhance error handling at each service layer, with a global middleware error handler for unified responses.
- Implement DLQs (Dead Letter Queues) for failed processes during crawling.
  
### Integration

- **Message Queueing**: Communicates between microservices using systems like Azure Service Bus or RabbitMQ.
- **API Communication**: Microservices can communicate via gRPC or REST as an alternative.

### CI/CD Pipeline

Use the `WebCrawler/Dockerfile` to build Docker images in your CI/CD pipeline. These images can then be pushed to cloud container registries and deployed to Kubernetes clusters (e.g., Azure Kubernetes Service - AKS).

### Deployment & Monitoring

### Kubernetes

The application can be deployed on any Kubernetes cluster (AKS, EKS, etc.).

### Observability Tools

- **Log Collection**: Tools like Datadog or Kibana (elastic-search) can gather logs from the Kubernetes PODs.
- **Metrics & Alerts**: Prometheus and Grafana can monitor the health of PODs and clusters, along with setting up alerts for threshold breaches.
- **Tracing**: Open-telemetry tools like Honeycomb to trace requests across services for in-depth analysis.

### Persistent Storage

- Store the Downloaded HTML file in an object based storage such as Amazon S3.
- Store crawler state and results in a database for query-based analysis.
  - Enable the crawling process to be paused and resumed by storing state.

## Future Improvements

### Optimized Scalable Crawling

- A separate DNS resolver to cache the IP and improve performance.
- Introduce a queuing mechanism (e.g., URL Frontier) to manage the crawling process more efficiently.
- Integrate `robots.txt` for honoring website-specific crawling rules.
- Rate Limiting Rules per Website: Implement rate limiting to avoid overloading the target website and to comply with its usage policies. This can be done using targeted queues with specific rules tied to each website.
  - Politeness queue: Mapping between the hostname and the queue to have the tailored rules.
- Distributed Crawling: Have the crawler deployed in different geographical locations which will help in reducing the latency.

### Maintainability

- Implement a centralized logging system to monitor the application's health and performance.
- Create domain specific objects to decouple the business logic from the infrastructure. 
  - Separate mappers for converting the domain objects to Infra objects.

### Fault tolerance

- Implement Circuit breaker pattern to handle any system failures gracefully.
- Ensure no single point of failure in the system by replicating the components where needed.

### Extensibility

- Advanced Content Extraction: Scraping Specific Content: Implement options to scrape and structure specific data such as titles, meta descriptions, headers, images, and other custom elements based on user-specified selectors or patterns.
- Dynamic Content Handling: Use headless browsers like Puppeteer or Selenium to handle and crawl dynamic content generated by JavaScript.
- Duplication Detection service: To validate the freshness of the downloaded files.


### Improved Result Formatting

- Provide more user-friendly and customizable output formats.
