# Simple web site crawler
This is a simple component that will crawl through a website (like https://www.bbc.co.uk/), find sub-links, and then crawl those pages as well. 

# Functionalities
- Only links that fall under the domain of the parent site will be accepted.

# Pre-requisites
- .Net SDK 8.0


# Usage
1. You can download as Zip or clone this repo using git
```shell
git clone https://github.com/dpgithub87/WebCrawler.git
````
Zip link: https://github.com/dpgithub87/WebCrawler/archive/refs/heads/main.zip

2. To run the project in any terminal:
Go to the project root directory and run the following command:
```shell
 dotnet run --project WebCrawler.Executor/WebCrawler.Executor.csproj --url "https://bing.com" --maxdepth 1 --format "json"
```
You can find the output in the Output folder in this directory `WebCrawler/WebCrawler.Executor/Output`
3. To run in docker:
   you can navigate to the solution root folder and execute the below commands to get interactive docker run (keeps STDIN open), Environment variable "Development" is required only to run in dev environment.
```shell
docker build -t webcrawler-executor:v1.0 . 
docker run --name web-crawler-container-unique12 -it -e ASPNETCORE_ENVIRONMENT=Development webcrawler-executor:v1.0 --url "https://bing.com" --maxdepth 1 --format "json"
```
To download the output from docker container to the local machine, you can use the below command in a different terminal:
```shell
docker cp web-crawler-container-unique12:/app/Output .
```

Docker arguments:
-t : tag
-rm : remove the container after it exits
-d : detach, Run container in background and print container ID
-it: interactive terminal, keeps the STDIN
Note: For first time run, this will take time as it downloads base aspnet 8.0 image as well as the SDK 8.0 image, Subsequent runs will be faster as it uses the cached images unless there are any modifications

# System Design
- Crawler-Executor: This is the main component handles the crawling process.
  - It uses the WebDownloader to download the HTML content of the page.
  - It uses the UriExtractor to extract the links from the HTML content.
  - It uses the background service to parallelize the crawling process.
- Fault tolerance: Polly retries are configured to handle transient errors. The default policy is to retry 3 times with an exponential backoff.

### Integration
  - Message Queuing systems such as Azure Service Bus used for communication between microservices
  - gRPC or REST could be used as an alternative communication mechanism between microservices
### Cache
  - Implement any distributed cache (such as Redis) to cache the upstream API responses.
  - Have to limit the maximum number of Pages based on the size. Alternatively, you can go for higher subscription in any PAAS model cloud cache.
 ### Cache
- Implement any distributed cache (such as Redis) to cache the upstream API responses
- Have to limit the maximum number of Pokemons based on the intended consumption of the API. Alternatively, you can go for higher subscription in any PAAS model cloud cache.

### Docker Image creation via pipelines
You can use the WebCrawler.Executor/Dockerfile in any build pipelines to build images that can be published to any cloud container registry, then deployed to the Kubernetes cluster by Release pipelines.
Sample Tech stack: Azure DevOps build pipeline, Release pipeline, Azure Container Registry & AKS

### Deployment & Observability
- Deploy it in any of the Kubernetes services such as AKS or EKS or private hosted Kubernetes
- Deploy any log collection agent such as Datadog or kibana to collect the logs from the PODs
- Reports to show the health of the PODs, Services and the Cluster
- Any Alerting mechanism if threshold limits reach on the usage of the Pods / Cluster
- Any monitoring tool such as Prometheus or Grafana to monitor the health of the PODs, Services and the Cluster
- Any tracing tool such as Honeycomb to trace the requests across the services

Project Components:
![img_1.png](img_1.png)

# Future Improvements
- The crawler could be made more efficient by using a queue to manage the links to be crawled.
  - URL Frontier could be used to manage the queue.
  - robots.txt could be used to determine the rules (like the rate of crawling) for each site.
- The WebDownloader/UriExtractor could be deployed as a separate micro service to allow for more efficient crawling of multiple sites.
  - This would allow for the WebDownloader/UriExtractor to be scaled independently of the crawler.
- The crawler-executor itself could be deployed as a separate Cron job that monitors a queue for new sites to crawl.
  - This allows parallel crawling of multiple sites; in addition to the background service that crawls the site.
- Better error handling could be implemented. Each layer of the services could have its own error handling mechanism.
  - This would allow for more detailed error messages to be returned to the user.
  - DLQs could be used to handle errors that occur during the crawling process.
- Results / state could be stored in a database for future reference.
  - This would allow for the results to be queried and analysed.
  - The state of the crawler could be stored in the database to allow for pausing and resuming of the crawling process.
- Results could be formatted in a more user-friendly way.

