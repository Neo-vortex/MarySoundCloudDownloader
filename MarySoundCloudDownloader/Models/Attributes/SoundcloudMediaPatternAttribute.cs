namespace MarySoundCloudDownloader.Models.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class SoundcloudMediaPatternAttribute : Attribute
{
    public string Description { get; }
    public PatternType Type { get; }

    public SoundcloudMediaPatternAttribute(PatternType type, string description = "")
    {
        Type = type;
        Description = description;
    }

    public enum PatternType
    {
        MediaUrl,
        PlaylistUrl,
        StreamUrl,
        Other
    }
}