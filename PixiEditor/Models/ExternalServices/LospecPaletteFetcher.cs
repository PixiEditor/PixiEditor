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
        public static async Task<PaletteList> FetchPage(int page, string sortingType = "default", string[] tags = null, 
            ColorsNumberMode colorsNumberMode = ColorsNumberMode.Any, int colorNumber = 8)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = @$"{LospecApiUrl}/load?colorNumberFilterType={colorsNumberMode.ToString().ToLower()}&page={page}&sortingType={sortingType}&tag=";
                    
                    if(tags != null && tags.Length > 0)
                    {
                        url += $"{string.Join(',', tags)}";
                    }

                    if(colorsNumberMode != ColorsNumberMode.Any)
                    {
                        url += $"&colorNumber={colorNumber}";
                    }

                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        var obj = JsonConvert.DeserializeObject<PaletteList>(content);

                        obj.FetchedCorrectly = obj.Palettes != null;
                        if (obj.Palettes != null)
                        {
                            foreach (var palette in obj.Palettes)
                            {
                                ReadjustColors(palette.Colors);
                            }
                        }

                        return obj;
                    }
                }
            }
            catch(HttpRequestException)
            {
                return new PaletteList() { FetchedCorrectly = false };
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
