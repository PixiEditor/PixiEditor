﻿namespace PixiEditor.Models.Services.NewsFeed;

internal enum NewsType
{
    NewVersion,
    YtVideo,
    BlogPost,
    OfficialAnnouncement,
    Misc
}

internal record News
{
    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public NewsType Type { get; set; }
    public string Url { get; set; }
}