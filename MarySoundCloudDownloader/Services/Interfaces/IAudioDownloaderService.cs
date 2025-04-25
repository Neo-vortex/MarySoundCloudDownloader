using MarySoundCloudDownloader.Helpers;
using MarySoundCloudDownloader.Models;

namespace MarySoundCloudDownloader.Services.Interfaces;

public interface IAudioDownloaderService
{
    public Task<RequestResult<DownloadResult>> DownloadAsync(string url,CancellationToken cancellationToken);
}