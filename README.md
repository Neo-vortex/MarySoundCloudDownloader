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
![image](https://github.com/user-attachments/assets/a90ee3ff-67f1-471e-9662-317e5534ec9b)
![image](https://github.com/user-attachments/assets/eb9d18be-c533-434f-8270-a9f71aa8a69c)
![image](https://github.com/user-attachments/assets/ce6b0c9c-7682-4769-900f-f1629a908487)

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

##Todo
Will try to serve it somewhere cheap :)
