using System.Threading.RateLimiting;
using MarySoundCloudDownloader.Components;
using MarySoundCloudDownloader.Services;
using MarySoundCloudDownloader.Services.Implementation;
using MarySoundCloudDownloader.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options =>
{
    options.DetailedErrors = true;
});
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.AddTokenBucketLimiter("GlobalPolicy", limiterOptions =>
    {
        limiterOptions.TokenLimit = 25;         
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;          
        limiterOptions.ReplenishmentPeriod = TimeSpan.FromMinutes(1); 
        limiterOptions.TokensPerPeriod = 25;      
        limiterOptions.AutoReplenishment = true; 
    });
});
builder.Services.AddHostedService<WwwRootCleanupService>();
builder.Services.AddScoped<IAudioDownloaderService, AudioDownloaderService>();
builder.Services.AddSingleton<IBrowserService, ChromeDriverService>();
builder.Services.ActivateSingleton<IBrowserService>();
var app = builder.Build();
app.UseRateLimiter();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireRateLimiting("GlobalPolicy");

app.Run();