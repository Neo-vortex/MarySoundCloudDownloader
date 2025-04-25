using System.Text.RegularExpressions;
using MarySoundCloudDownloader.Models.Attributes;

namespace MarySoundCloudDownloader.Models.SoundCloudMediaLinksRegex;

public partial class SoundCloudRegexPatterns
{
    [GeneratedRegex(@"https:\/\/[a-z0-9-]+\.sndcdn\.com\/(?:playlist\/[a-zA-Z0-9]+\.[0-9]+\.mp3(?:\/playlist\.m3u8)?|media\/[^""]+\.[0-9]+\.mp3)(?:\?[^""]+)?", 
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    [SoundcloudMediaPattern(SoundcloudMediaPatternAttribute.PatternType.MediaUrl)]
    public static partial Regex SndCdnMediaUrl();
    
    [GeneratedRegex(@"https:\/\/playback\.media-streaming\.soundcloud\.cloud\/[a-zA-Z0-9]+\/[a-z]+_[0-9]+k\/[a-f0-9-]{8}(?:-[a-f0-9-]{4}){3}-[a-f0-9-]{12}\/playlist\.m3u8\?expires=[0-9]+&Policy=[^&]+&Signature=[^&]+&Key-Pair-Id=[a-zA-Z0-9]+", 
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    [SoundcloudMediaPattern(SoundcloudMediaPatternAttribute.PatternType.StreamUrl)]
    public static partial Regex PlaybackStreamingUrl();
}