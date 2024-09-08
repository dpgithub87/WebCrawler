using System.Diagnostics;

namespace WebCrawler.Executor.Integration.Tests;

public class WebCrawlerIntegrationTests
{
    [Fact]
    public async void ExecuteCrawlerAndVerifyTheOutputFile()
    {
        // Define the output directory path
        var workingDirectory = Path.GetFullPath("../../../../WebCrawler.Executor");
        var outputDirectory = $"{workingDirectory}/Output";
        const string urlToCrawl = "https://www.google.com";
        const string outputFormat = "json";
        const string maxDepth = "0";
        
        // Set up the FileSystemWatcher to monitor the output directory
        var fileCreated = new TaskCompletionSource<string>();
        using var watcher = new FileSystemWatcher(outputDirectory);
        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
        watcher.Created += (sender, args) => { fileCreated.SetResult(args.FullPath); };
        watcher.EnableRaisingEvents = true;

        // Start the console application
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project WebCrawler.Executor.csproj --url {urlToCrawl} --maxdepth {maxDepth} --format {outputFormat}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = workingDirectory
            }
        };

        process.Start();
        
        // Wait for the FileSystemWatcher to detect the new file
        var newFilePath = await fileCreated.Task;
        
        // Send a cancellation signal to stop the application
        process.Kill();

        // Verify the output file exists
        Assert.True(File.Exists(newFilePath), "Output file was not created.");

        // Verify the content of the output file
        var outputContent = await File.ReadAllTextAsync(newFilePath);
        Assert.Contains("https://www.google.com", outputContent);
        
        // Clean up the output file
        File.Delete(newFilePath);
    }
    
}