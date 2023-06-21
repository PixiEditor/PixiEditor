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
    public string ShortDescription { get; set; }

    public NewsType NewsType { get; set; }
    public string Url { get; set; }
    public DateTime Date { get; set; }
    public string CustomIconUrl { get; set; }

    [JsonIgnore]
    public string ResolvedIconUrl => CustomIconUrl ?? $"/Images/News/{NewsType.GetDescription()}";
}
