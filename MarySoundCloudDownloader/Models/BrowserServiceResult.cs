using System.Collections.Concurrent;

namespace MarySoundCloudDownloader.Models;

public class BrowserServiceResult
{
    public required ConcurrentBag<string> ProcessSoundCloudUrl { get; set; }
    
    public  string? TrackName { get; set; }
    
    public  string? TrackImage { get; set; }
}