namespace WebCrawler.Executor.Helpers;

public static class OutputPathHelper
{
    public static string GetProjectRoot()
    {
        // Get the base directory of the application
        var baseDirectory = AppContext.BaseDirectory;

        // Navigate up the directory tree to find the project root
        var directoryInfo = new DirectoryInfo(baseDirectory);
        while (directoryInfo != null && !directoryInfo.GetFiles("*.csproj").Any())
        {
            directoryInfo = directoryInfo.Parent;
        }

        if (directoryInfo == null)
        {
            throw new InvalidOperationException("Project root could not be found.");
        }

        return directoryInfo.FullName;
    }
}