﻿using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Platform;
using PixiEditor.UpdateModule;

namespace PixiEditor.Models.Services.NewsFeed;

internal class NewsProvider
{
    private const int MaxNewsCount = 20;
    private const string FeedUrl = "https://raw.githubusercontent.com/PixiEditor/news-feed/main/";

    private List<int> _lastCheckedIds = new List<int>();

    public NewsProvider()
    {
        _lastCheckedIds = IPreferences.Current.GetPreference(PreferencesConstants.LastCheckedNewsIds, new List<int>());
    }

    public async Task<List<News>?> FetchNewsAsync()
    {
        List<News> allNews = new List<News>();
        await FetchFrom(allNews, "shared.json");
        await FetchFrom(allNews, $"{IPlatform.Current.Id}.json");

        var sorted = allNews.OrderByDescending(x => x.Date).Take(MaxNewsCount).ToList();
        MarkNewOnes(sorted);
        return sorted;
    }

    private async Task FetchFrom(List<News> output, string fileName)
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
        HttpResponseMessage response = await client.GetAsync($"{FeedUrl}{fileName}");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            string content = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<News>>(content);
            output.AddRange(list);
        }
    }

    private void MarkNewOnes(List<News> list)
    {
        foreach (var news in list)
        {
            if (news.GetIdentifyingNumber() is var num && !_lastCheckedIds.Contains(num))
            {
                news.IsNew = true;
                _lastCheckedIds.Add(num);
                if (_lastCheckedIds.Count > MaxNewsCount)
                {
                    _lastCheckedIds.RemoveAt(0);
                }
            }
        }

        IPreferences.Current.UpdatePreference(PreferencesConstants.LastCheckedNewsIds, _lastCheckedIds);
    }
}
