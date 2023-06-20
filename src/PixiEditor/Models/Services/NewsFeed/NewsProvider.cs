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
            var list = JsonConvert.DeserializeObject<List<News>>(content);
            list.Add(new News(){ Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, " +
                                         "nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec", Date = DateTime.Now,
                ShortDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam euismod, nisl eget ultricies ultrices, nisl nisl ultricies nisl, nec" ,
                CustomIconUrl = "https://raw.githubusercontent.com/PixiEditor/PixiEditor/master/src/PixiEditor/Images/SocialMedia/WebsiteIcon.png"
            });

            return list;
        }

        return null;
    }
}
