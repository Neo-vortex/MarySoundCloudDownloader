using System.Collections.Concurrent;
using MarySoundCloudDownloader.Models;

namespace MarySoundCloudDownloader.Services.Interfaces;

public interface IBrowserService : IDisposable
{
    public Task<BrowserServiceResult> ProcessSoundCloudUrlAsync(string url, CancellationToken cancellationToken);
} 