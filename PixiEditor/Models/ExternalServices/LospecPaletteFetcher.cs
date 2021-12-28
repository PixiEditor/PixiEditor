using Newtonsoft.Json;
using PixiEditor.Models.DataHolders;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PixiEditor.Models.ExternalServices
{
    public static class LospecPaletteFetcher
    {
        public const string LospecApiUrl = "https://lospec.com/palette-list";
        public static async Task<PaletteList> FetchPage(int page)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = @$"{LospecApiUrl}/load?colorNumberFilterType=any&page={page}&tag=&sortingType=default";
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PaletteList>(content);
                }
            }

            return null;
        }
    }
}
