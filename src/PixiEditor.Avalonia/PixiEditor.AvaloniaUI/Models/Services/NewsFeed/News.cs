using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using PixiEditor.AvaloniaUI.Helpers.Converters.JsonConverters;
using PixiEditor.Extensions.Helpers;

namespace PixiEditor.AvaloniaUI.Models.Services.NewsFeed;

[JsonConverter(typeof(DefaultUnknownEnumConverter), (int)Misc)]
internal enum NewsType
{
    [Description("NewVersion.png")]
    NewVersion,
    [Description("YouTube.png")]
    YtVideo,
    [Description("Article.png")]
    BlogPost,
    [Description("OfficialAnnouncement.png")]
    OfficialAnnouncement,
    [Description("Misc.png")]
    Misc
}

internal record News
{
    public string Title { get; init; } = string.Empty;
    public NewsType NewsType { get; init; } = NewsType.Misc;
    public string Url { get; init; }
    public DateTime Date { get; init; }
    public string CoverImageUrl { get; init; } = string.Empty;

    [JsonIgnore]
    public string ResolvedIconUrl => $"/Images/News/{NewsType.GetDescription()}";

    [JsonIgnore]
    public bool IsNew { get; set; } = false;

    public int GetIdentifyingNumber()
    {
        MD5 md5Hasher = MD5.Create();
        string data = Title + Url + Date.ToString(CultureInfo.InvariantCulture) + CoverImageUrl;
        var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToInt32(hashed, 0);
    }
}
