namespace MarySoundCloudDownloader.Helpers;

public static class UrlEncoder
{
    public static string EncodeUrl(string rawUrl)
    {
    
        var lastSlashIndex = rawUrl.LastIndexOf('/');
        if (lastSlashIndex < 0)
            return rawUrl; 
        
        var baseUrl = rawUrl.Substring(0, lastSlashIndex + 1);
        var fileName = rawUrl.Substring(lastSlashIndex + 1);
        
        var encodedFileName = Uri.EscapeDataString(fileName);
        
        return baseUrl + encodedFileName;
    }

    public static string EncodeFileUrl(string baseUrl, string fileName)
    {
        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";
            
        return baseUrl + Uri.EscapeDataString(fileName);
    }
}