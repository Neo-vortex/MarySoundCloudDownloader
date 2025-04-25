# Mary SoundCloud Downloader 🎧

**Convert SoundCloud tracks to high-quality MP3 files instantly using a sleek Blazor Server interface.**

> ⚠️ For educational use only. This project is not affiliated with SoundCloud.

---

## 🚀 Features

- 🔗 Paste any public **SoundCloud track URL**
- 🧠 Auto-cleans and validates input URLs
- 📥 Downloads tracks as high-quality MP3 files
- 🖼️ Displays track metadata and artwork
- 📱 Generates a **QR code** for easy mobile download
- ♻️ Tracks are temporary – links last for **20 minutes**
- 🛡️ Built-in **rate limiting** to prevent abuse

---

## 🧱 Tech Stack

- Blazor Server (.NET 9)
- Selenium WebDriver (ChromeDriver)
- FFmpeg for audio processing
- MemoryCache for temporary results
- Rate limiting via ASP.NET Core middleware
- JS Interop for clipboard access

---

## 🛠️ Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Chrome browser](https://www.google.com/chrome/)
- [FFmpeg installed](https://ffmpeg.org/download.html) and available in system PATH

### Clone & Run

```bash
git clone https://github.com/your-username/MarySoundCloudDownloader.git
cd MarySoundCloudDownloader
dotnet run
```


---

## 🔐 Rate Limiting

Mary SoundCloud Downloader includes a token bucket limiter to ensure fair usage:

- ✅ **25 requests/minute**
- ⏳ Additional requests are queued (max 10)
---

## 🙏 Disclaimer

This tool is meant for educational purposes only.  
Always respect artist rights and avoid distributing copyrighted content.
