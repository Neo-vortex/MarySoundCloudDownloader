using System.Reflection;
using System.Text.RegularExpressions;
using MarySoundCloudDownloader.Models.Attributes;
using MarySoundCloudDownloader.Models.SoundCloudMediaLinksRegex;

namespace MarySoundCloudDownloader.Helpers;

public static class SoundCloudRegexUtility
{
    /// <summary>
    /// Gets all methods in SoundCloudRegexPatterns with SoundcloudMediaPattern attribute
    /// </summary>
    public static IEnumerable<SoundCloudPatternInfo> GetSoundCloudPatterns()
    {
        var patternClass = typeof(SoundCloudRegexPatterns);
        
        foreach (var method in patternClass.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            var soundcloudAttr = method.GetCustomAttribute<SoundcloudMediaPatternAttribute>();
            if (soundcloudAttr == null) continue;

            var regexAttr = method.GetCustomAttribute<GeneratedRegexAttribute>();
            if (regexAttr == null) continue;

            yield return new SoundCloudPatternInfo(
                MethodName: method.Name,
                PatternType: soundcloudAttr.Type,
                Description: soundcloudAttr.Description,
                Pattern: regexAttr.Pattern,
                Options: regexAttr.Options,
                RegexInstance: (Regex)method.Invoke(null, null)
            );
        }
    }

    /// <summary>
    /// Gets a specific pattern by type
    /// </summary>
    public static SoundCloudPatternInfo? GetPatternByType(
        SoundcloudMediaPatternAttribute.PatternType type)
    {
        return GetSoundCloudPatterns()
            .FirstOrDefault(p => p.PatternType == type);
    }

    /// <summary>
    /// Prints all available patterns to console
    /// </summary>
    public static void PrintAvailablePatterns()
    {
        var patterns = GetSoundCloudPatterns().ToList();
        
        Console.WriteLine($"Available SoundCloud Patterns ({patterns.Count}):");
        Console.WriteLine("====================================");
        
        foreach (var pattern in patterns)
        {
            Console.WriteLine($"\n[{pattern.PatternType}] {pattern.MethodName}");
            Console.WriteLine($"Description: {pattern.Description}");
            Console.WriteLine($"Pattern: {pattern.Pattern}");
            Console.WriteLine($"Options: {pattern.Options}");
            Console.WriteLine("------------------------------------");
        }
    }

    /// <summary>
    /// Tests a URL against all patterns and returns matching pattern info
    /// </summary>
    public static SoundCloudPatternInfo? TestUrl(string url)
    {
        return GetSoundCloudPatterns()
            .FirstOrDefault(p => p.RegexInstance.IsMatch(url));
    }
}

public record SoundCloudPatternInfo(
    string MethodName,
    SoundcloudMediaPatternAttribute.PatternType PatternType,
    string Description,
    string Pattern,
    RegexOptions Options,
    Regex RegexInstance
);