namespace MarySoundCloudDownloader.Helpers;

public static class TempPathHelper
{
    public static string GetRandomTempPath(string prefix = "temp_", string extension = "")
    {
        var tempDir = Path.GetTempPath();
        var randomName = prefix + Path.GetRandomFileName();
        if (string.IsNullOrEmpty(extension))
        {
            randomName = Path.GetFileNameWithoutExtension(randomName);
        }
        else
        {
            extension = extension.StartsWith(".") ? extension : "." + extension;
            randomName = Path.ChangeExtension(randomName, extension);
        }
        
        Directory.CreateDirectory(tempDir + randomName);
        return Path.Combine(tempDir, randomName);
    }

    public static void CleanupTempPath(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up temp path {path}: {ex.Message}");
        }
    }
}