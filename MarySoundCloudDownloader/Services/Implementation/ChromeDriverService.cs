using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using MarySoundCloudDownloader.Models;
using MarySoundCloudDownloader.Services.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using static System.GC;
using LogLevel = OpenQA.Selenium.LogLevel;

namespace MarySoundCloudDownloader.Services.Implementation;

public class ChromeDriverService : IBrowserService
{
    private readonly ChromeDriver _driver;
    private readonly ILogger<ChromeDriverService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IHttpClientFactory httpClientFactory;

    public ChromeDriverService(ILogger<ChromeDriverService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        this.httpClientFactory = httpClientFactory;
        var options = new ChromeOptions
        {
            PageLoadStrategy = PageLoadStrategy.Normal
        };

        options.AddArgument("--disable-web-security");
        options.AddArgument("--disable-site-isolation-trials");
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--headless=new");
        options.AddArgument("--mute-audio");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--window-size=1920,1080");
        options.SetLoggingPreference(LogType.Performance, LogLevel.All);

        _logger.LogInformation("Initializing ChromeDriver...");
        _driver = new ChromeDriver(options);
        _driver.Manage().Window.Maximize();
    }

    public async Task<BrowserServiceResult> ProcessSoundCloudUrlAsync(string url, CancellationToken cancellationToken)
    {
        return await ExecuteInNewTabAsync(async driver =>
        {
            var logs = new ConcurrentBag<string>();

            _logger.LogInformation("Loading: {Url}", url);
            await driver.Navigate().GoToUrlAsync(url);

            //sometimes it takes time for the cookie acceptance policy to show up
            await Task.Delay(1000, cancellationToken);

            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                        const el = document.getElementById('onetrust-policy-text');
                        if (el) el.remove();
                    ");
            try
            {
                                ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    const target = document.querySelector('a.playButton');
                    if (!target) return;

                    const rect = target.getBoundingClientRect();
                    const centerX = rect.left + rect.width / 2;
                    const centerY = rect.top + rect.height / 2;

                    let topElement = document.elementFromPoint(centerX, centerY);
                    let tries = 5;

                    while (topElement && topElement !== target && !target.contains(topElement) && tries-- > 0) {
                        console.warn('Removing:', topElement);
                        topElement.remove();
                        topElement = document.elementFromPoint(centerX, centerY);
                    }
                ");
                _logger.LogInformation("Removing ot-fade-in elements...");
            }
            catch (Exception ex)
            {
                logs.Add($"Error removing elements: {ex.Message}");
                _logger.LogError(ex, "Error removing elements");
            }

            _logger.LogInformation("Extracting image and track name...");
            var trackPictureList = driver.FindElements(By.ClassName("sc-artwork"));
            var trackPicture = trackPictureList.FirstOrDefault(x => x.TagName == "span");
            var actions2 = new Actions(driver);
            actions2.MoveToElement(trackPicture).Perform();
            var ariaLabel = trackPicture.GetAttribute("aria-label");
            var styleAttribute = ExtractBackgroundImageUrl(trackPicture.GetAttribute("style"));
            styleAttribute =
                Convert.ToBase64String(await httpClientFactory.CreateClient()
                    .GetByteArrayAsync(styleAttribute, cancellationToken));

            try
            {
                _logger.LogInformation("Clicking play button...");
                var playButton = driver.FindElement(By.ClassName("playButton"));
                var actions = new Actions(driver);
                actions.MoveToElement(playButton).Perform();
                playButton.Click();
                await Task.Delay(2000, cancellationToken);
                playButton.Click();
                logs.Add("Play button clicked successfully");
                _logger.LogInformation("Play button clicked successfully");
            }
            catch (Exception ex)
            {
                logs.Add($"Error clicking play button: {ex.Message}");
                _logger.LogError(ex, "Error clicking play button");
            }

            // wait fot the links to fireup

            _logger.LogInformation("Capturing network traffic...");
            var performanceLogs = driver.Manage().Logs.GetLog("performance");
            foreach (var log in performanceLogs)
            {
                logs.Add(log.Message);
            }

            return new BrowserServiceResult()
            {
                ProcessSoundCloudUrl = logs,
                TrackImage = styleAttribute,
                TrackName = ariaLabel
            };
        });
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
        _driver?.Quit();
        _driver?.Dispose();
        SuppressFinalize(this);
    }

    private static string ExtractBackgroundImageUrl(string style)
    {
        var match = Regex.Match(
            style,
            """background-image:\s*url\(["']?(.*?)["']?\)""",
            RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private async Task<BrowserServiceResult> ExecuteInNewTabAsync(Func<ChromeDriver, Task<BrowserServiceResult>> action)
    {
        await _semaphore.WaitAsync();
        try
        {
            /*
            // Store current window handle
            var originalWindow = _driver.CurrentWindowHandle;

            // Open new tab
            ((IJavaScriptExecutor)_driver).ExecuteScript("window.open()");
            var windows = _driver.WindowHandles;
            _driver.SwitchTo().Window(windows.Last());
            */

            try
            {
                var result = await action(_driver);
                return result;
            }
            finally
            {
                // Close current tab and switch back to original
                //_driver.Close();
                /*_driver.SwitchTo().Window(originalWindow);*/
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}