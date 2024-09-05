# Simple Web Crawler

A lightweight and efficient web crawler that recursively crawls through a website, retrieves sub-links, and continues to crawl those pages, all while staying within the domain of the parent site. For example, you can crawl websites like BBC.

## Features

- Crawls through websites, gathering sub-links.
- Only crawls links within the parent domain.
- Supports output in various formats such as JSON.
- Can be run locally or within a Docker container.

## Prerequisites

- .NET SDK 8.0

## Usage

### 1. Clone or Download the Repository

You can either download this repository as a zip file or clone it via git.
```bash
git clone https://github.com/dpgithub87/WebCrawler.git
```
### 2. Run the Project Locally

To run the crawler, navigate to the root directory and execute the appropriate command in your terminal.
```bash
dotnet run --project WebCrawler.Executor/WebCrawler.Executor.csproj --url "https://bing.com" --maxdepth 1 --format "json"
```
**Output**: The results will be saved in the `Output` folder located in `WebCrawler/WebCrawler.Executor/Output`.

### 3. Running the Crawler in Docker

To run the crawler within Docker, follow these steps:

1. Build the Docker image.
```bash
docker build -t webcrawler-executor:v1.0 .
```
2. Run the Docker container.
```bash
docker run --name web-crawler-container -it -e ASPNETCORE_ENVIRONMENT=Development webcrawler-executor:v1.0 --url "https://bing.com" --maxdepth 1 --format "json"
```
3. Retrieve the output from the Docker container to your local machine.
```bash
docker cp web-crawler-container:/app/Output .
```
**Docker Arguments Explanation**:

- `-t`: Tag for naming the image.
- `--rm`: Automatically remove the container once it stops.
- `-d`: Run container in detached mode.
- `-it`: Interactive terminal to keep STDIN open.

**Note**: The first run may take some time as Docker downloads the base ASP.NET 8.0 and SDK images. Subsequent runs will be faster due to caching.

## System Architecture

### Core Components

- **Crawler Executor**: This is the main component responsible for orchestrating the crawling process.
    - **WebDownloader**: Fetches the HTML content of each page.
    - **UriExtractor**: Extracts all valid URLs from the HTML content.
    - **Background Service**: Manages concurrent crawling using parallel tasks.

### Fault Tolerance

- **Polly Library**: Configured with retries (3 times by default) using exponential backoff to handle transient errors.

### Integration

- **Message Queueing**: Communicates between microservices using systems like Azure Service Bus.
- **API Communication**: Microservices can communicate via gRPC or REST as an alternative.

### Caching

- **Distributed Caching**: Leverage Redis or similar services to cache upstream API responses.
- **Page Limit**: You can impose a limit on the number of pages based on size to control resource consumption.

## CI/CD Pipeline

Use the `WebCrawler.Executor/Dockerfile` to build Docker images in your CI/CD pipeline. These images can then be pushed to cloud container registries and deployed to Kubernetes clusters (e.g., Azure Kubernetes Service - AKS).

## Deployment & Monitoring

### Kubernetes

The application can be deployed on any Kubernetes cluster (AKS, EKS, etc.).

### Observability Tools

- **Log Collection**: Tools like Datadog or Kibana can gather logs from the Kubernetes PODs.
- **Metrics & Alerts**: Prometheus and Grafana can monitor the health of PODs and clusters, along with setting up alerts for threshold breaches.
- **Tracing**: Use tools like Honeycomb to trace requests across services for in-depth analysis.

## Future Improvements

### Optimized Crawling

- Introduce a queuing mechanism (e.g., URL Frontier) to manage the crawling process more efficiently.
- Integrate `robots.txt` for honoring website-specific crawling rules.

### Parallel Processing

- Implement the crawler-executor as a Cron job to monitor a queue for new sites, enabling parallel crawling.

### Microservice Architecture

- Extract the WebDownloader and UriExtractor as independent microservices to enhance scalability and efficiency.

### Error Handling

- Enhance error handling at each service layer, with a middleware for unified responses.
- Implement DLQs (Dead Letter Queues) for failed processes during crawling.

### Persistent Storage

- Store crawler state and results in a database for query-based analysis.
- Enable the crawling process to be paused and resumed by storing state.

### Improved Result Formatting

- Provide more user-friendly and customizable output formats.