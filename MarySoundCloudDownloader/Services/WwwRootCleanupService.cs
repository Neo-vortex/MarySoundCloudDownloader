namespace MarySoundCloudDownloader.Services;

public class WwwRootCleanupService : BackgroundService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<WwwRootCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(20);
    private readonly TimeSpan _fileAgeThreshold = TimeSpan.FromMinutes(20);

    public WwwRootCleanupService(
        IWebHostEnvironment env,
        ILogger<WwwRootCleanupService> logger)
    {
        _env = env;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WwwRoot Cleanup Service is starting.");
        
        using var timer = new PeriodicTimer(_cleanupInterval);
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CleanupOldFilesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during wwwroot cleanup");
            }
        }
    }

    private async Task CleanupOldFilesAsync(CancellationToken cancellationToken)
    {
        var wwwrootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
        var cutoffTime = DateTime.UtcNow - _fileAgeThreshold;
        
        _logger.LogInformation("Starting wwwroot cleanup at {Time}", DateTime.UtcNow);

        if (!Directory.Exists(wwwrootPath))
        {
            _logger.LogWarning("wwwroot directory not found at {Path}", wwwrootPath);
            return;
        }

        var files = Directory.GetFiles(wwwrootPath, "*.mp3", SearchOption.AllDirectories);
        int deletedCount = 0;

        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < cutoffTime)
                {
                    fileInfo.Delete();
                    deletedCount++;
                    _logger.LogDebug("Deleted file: {File}", file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete file {File}", file);
            }
        }

        _logger.LogInformation("Cleanup completed. Deleted {Count} files.", deletedCount);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WwwRoot Cleanup Service is stopping.");
        await base.StopAsync(cancellationToken);
    }
}