using Newtonsoft.Json;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.Enums;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PixiEditor.Models.ExternalServices
{
    public static class LospecPaletteFetcher
    {
        public const string LospecApiUrl = "https://lospec.com/palette-list";

        public static async Task<Palette> FetchPalette(string slug)
        {
            try
            {
                using HttpClient client = new HttpClient();
                string url = @$"{LospecApiUrl}/{slug}.json";

                HttpResponseMessage response = await client.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<Palette>(content);

                    if (obj is { Colors: { } })
                    {
                        ReadjustColors(obj.Colors);
                    }

                    return obj;
                }
            }
            catch(HttpRequestException)
            {
                return null;
            }

            return null;
        }

        private static void ReadjustColors(List<string> colors)
        {
            for (int i = 0; i < colors.Count; i++)
            {
                colors[i] = colors[i].Insert(0, "#");
            }
        }
    }
}
