using System.Text.RegularExpressions;

namespace MarySoundCloudDownloader.Helpers;

public class M3UPlaylist
{
    public int Version { get; set; }
    public int TargetDuration { get; set; }
    public int MediaSequence { get; set; }
    public  string? PlaylistType { get; set; }
    public  string? InitSegmentUri { get; set; }
    public List<Segment> Segments { get; set; } = [];
}

public class Segment
{
    public double Duration { get; set; }
    public string Uri { get; set; }
}

public static class M3UParser
{
    public static M3UPlaylist Parse(string m3uContent)
    {
        using var reader = new StringReader(m3uContent);
        string? line;
        var playlist = new M3UPlaylist();
        double? nextSegmentDuration = null;

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();

            if (line.StartsWith("#EXT-X-VERSION:"))
            {
                playlist.Version = int.Parse(line["#EXT-X-VERSION:".Length..]);
            }
            else if (line.StartsWith("#EXT-X-TARGETDURATION:"))
            {
                playlist.TargetDuration = int.Parse(line["#EXT-X-TARGETDURATION:".Length..]);
            }
            else if (line.StartsWith("#EXT-X-MEDIA-SEQUENCE:"))
            {
                playlist.MediaSequence = int.Parse(line["#EXT-X-MEDIA-SEQUENCE:".Length..]);
            }
            else if (line.StartsWith("#EXT-X-PLAYLIST-TYPE:"))
            {
                playlist.PlaylistType = line["#EXT-X-PLAYLIST-TYPE:".Length..];
            }
            else if (line.StartsWith("#EXT-X-MAP:"))
            {
                var match = Regex.Match(line, "URI=\"([^\"]+)\"");
                if (match.Success)
                {
                    playlist.InitSegmentUri = match.Groups[1].Value;
                }
            }
            else if (line.StartsWith("#EXTINF:"))
            {
                var durationStr = line["#EXTINF:".Length..].TrimEnd(',');
                nextSegmentDuration = double.Parse(durationStr);
            }
            else if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
            {
                playlist.Segments.Add(new Segment
                {
                    Duration = nextSegmentDuration ?? 0,
                    Uri = line
                });
                nextSegmentDuration = null;
            }
        }

        return playlist;
    }
}