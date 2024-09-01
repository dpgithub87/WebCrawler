FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WebCrawler.Executor/WebCrawler.Executor.csproj", "WebCrawler.Executor/"]
RUN dotnet restore "WebCrawler.Executor/WebCrawler.Executor.csproj"
COPY . .
WORKDIR "/src/WebCrawler.Executor"
RUN dotnet build "WebCrawler.Executor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebCrawler.Executor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebCrawler.Executor.dll"]
