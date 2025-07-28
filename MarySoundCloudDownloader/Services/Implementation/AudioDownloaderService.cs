using System.Diagnostics;
using System.Text.Encodings.Web;
using MarySoundCloudDownloader.Helpers;
using MarySoundCloudDownloader.Models;
using MarySoundCloudDownloader.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using UrlEncoder = MarySoundCloudDownloader.Helpers.UrlEncoder;


namespace MarySoundCloudDownloader.Services.Implementation;
public class AudioDownloaderService(ILogger<AudioDownloaderService> logger, IBrowserService browserService, IHttpClientFactory  httpClientFactory,
    IHostEnvironment  env , IMemoryCache cache,
    IHttpContextAccessor httpContextAccessor) : IAudioDownloaderService
{
    public async Task<RequestResult<DownloadResult>> DownloadAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var logs = await browserService.ProcessSoundCloudUrlAsync(url,cancellationToken);

            var playListUrl = string.Empty;
            var soundCloudPatterns = SoundCloudRegexUtility
                .GetSoundCloudPatterns()
                .ToList();

            foreach (var soundCloudPattern in soundCloudPatterns)
            {
                foreach (var log in logs.ProcessSoundCloudUrl)
                {
                    var result = soundCloudPattern.RegexInstance.Match(log);
                    if (!result.Success || !result.Value.Contains("playlist")) continue;
                    playListUrl = result.Value;
                    logger.LogInformation("Found media playlist URL: {MediaUrl}" , playListUrl);
                    break;
                }
            }
   
            var m3u8Content = await  httpClientFactory.CreateClient().GetStringAsync(playListUrl, cancellationToken);
            var playListInfo = M3UParser.Parse(m3u8Content);
            if (playListInfo.InitSegmentUri is not null)
            {
                playListInfo.Segments.Insert(0 ,new Segment()
                {
                    Uri = playListInfo.InitSegmentUri
                });
            }
            var tempDir = TempPathHelper.GetRandomTempPath();
            var downloadTasks = new List<Task>();
            var segmentFiles = new List<string>();
            var counter = 0;
            foreach (var segmentUrl in playListInfo.Segments)
            {
                var (segName, segExt) = GetFileInfoFromUrl(segmentUrl.Uri);
                var fileName = Path.Combine(tempDir, $"{counter++}{segName}.{segExt}");
                segmentFiles.Add(fileName);
                downloadTasks.Add(DownloadFileAsync(httpClientFactory.CreateClient(), segmentUrl.Uri, fileName));
            }
            await Task.WhenAll(downloadTasks);
            ConcatenateFiles(segmentFiles , Path.Combine(tempDir, "final"));
            RunFFmpeg(Path.Combine(tempDir, "final"), Path.Combine(tempDir, "final.mp3"));

            var filename = logs.TrackName + ".mp3";
            File.Copy(Path.Combine(tempDir, "final.mp3"),  Path.Combine(Path.Combine(env.ContentRootPath, "wwwroot"),filename), true);
            
            return new DownloadResult()
            {
                FilePath = UrlEncoder.EncodeUrl( httpContextAccessor.HttpContext!.Request.Scheme + "://" + httpContextAccessor.HttpContext!.Request.Host.Value + "/" + filename ),
                TrackName = logs.TrackName,
                TrackImage = logs.TrackImage,
            };
        }
        catch (Exception e)
        {
            return new Exception(e.Message, e);
        }
    }
    

    private static (string fileName, string extension) GetFileInfoFromUrl(string url)
    {
        try
        {
            var cleanUrl = url.Split('?')[0].Split('#')[0];
            var lastSegment = cleanUrl.Split('/').Last();

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(lastSegment);
            var extension = Path.GetExtension(lastSegment).TrimStart('.');

            if (string.IsNullOrEmpty(extension))
            {
                if (url.Contains(".mp4") || url.Contains("mp4a")) extension = "mp4";
                else if (url.Contains(".aac")) extension = "aac";
                else if (url.Contains(".m4s")) extension = "m4s";
                else extension = "mp3";
            }

            fileNameWithoutExt = string.Concat(fileNameWithoutExt.Split(Path.GetInvalidFileNameChars()));
            return (fileNameWithoutExt, extension.ToLower());
        }
        catch { return ("segment", "mp3"); }
    }

    private  async Task DownloadFileAsync(HttpClient client, string url, string fileName)
    {
        try
        {
            using (var response = await client.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                await using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }
            logger.LogInformation($"Downloaded: {Path.GetFileName(fileName)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading {url}: {ex.Message}");
        }
    }

    private  void ConcatenateFiles(List<string> inputFiles, string outputFile)
    {
        using var outputStream = File.Create(outputFile);
        foreach (var inputFile in inputFiles)
        {
            try
            {
                if (!File.Exists(inputFile)) continue;
                using var inputStream = File.OpenRead(inputFile);
                inputStream.CopyTo(outputStream);
            }
            catch (Exception ex) { logger.LogError($"Error adding {inputFile}: {ex.Message}"); }
        }
    }

    private bool RunFFmpeg(string inputFile, string outputFile)
    {
        try
        {
            const string ffmpegPath = "ffmpeg";

            // Step 1: Detect file format
            var formatCheck = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-i \"{inputFile}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            formatCheck.Start();
            string stderr = formatCheck.StandardError.ReadToEnd();
            formatCheck.WaitForExit();

            if (stderr.Contains("Audio: mp3"))
            {
                logger.LogInformation("Detected MP3 format. Copying to output.");
                File.Copy(inputFile, outputFile, overwrite: true);
                return File.Exists(outputFile);
            }


            var arguments = $"-i \"{inputFile}\" -codec:a libmp3lame -b:a 192k -ac 2 \"{outputFile}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            logger.LogInformation($"Running: ffmpeg {arguments}");
            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                return File.Exists(outputFile);
            }

            logger.LogInformation($"FFmpeg Error:\n{process.StandardError.ReadToEnd()}");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError($"FFmpeg execution error: {ex.Message}");
            return false;
        }
    }



}