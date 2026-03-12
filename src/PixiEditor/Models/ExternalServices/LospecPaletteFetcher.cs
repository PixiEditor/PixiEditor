using System.Net;
using System.Net.Http.Json;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Palettes;

namespace PixiEditor.Models.ExternalServices;

internal static class LospecPaletteFetcher
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
                var content = await response.Content.ReadFromJsonAsync<PaletteObject>();

                if (content is { Colors: not null })
                {
                    ReadjustColors(content.Colors);
                }

                return content.ToPalette();
            }
        }
        catch (HttpRequestException)
        {
            NoticeDialog.Show("FAILED_DOWNLOAD_PALETTE", "ERROR");
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
