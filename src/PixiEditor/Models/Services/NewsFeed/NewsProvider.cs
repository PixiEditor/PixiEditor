using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using PixiEditor.UpdateModule;

namespace PixiEditor.Models.Services.NewsFeed;

internal class NewsProvider
{
    private const int ProtocolVersion = 1;
    private const string FeedUrl = "https://raw.githubusercontent.com/PixiEditor/news-feed/main/";
    public async Task<List<News>?> FetchNewsAsync()
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
        HttpResponseMessage response = await client.GetAsync(FeedUrl + "shared.json");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            string content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<News>>(content);
        }

        return null;
    }
}
