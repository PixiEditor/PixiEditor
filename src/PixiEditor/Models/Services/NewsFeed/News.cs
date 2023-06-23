using System.ComponentModel;
using Newtonsoft.Json;

namespace PixiEditor.Models.Services.NewsFeed;

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
    public string Title { get; set; }
    public NewsType NewsType { get; set; } = NewsType.Misc;
    public string Url { get; set; }
    public DateTime Date { get; set; }

    public string CoverImageUrl { get; set; }

    [JsonIgnore]
    public string ResolvedIconUrl => $"/Images/News/{NewsType.GetDescription()}";
}
