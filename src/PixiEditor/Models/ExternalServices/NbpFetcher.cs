using System.Net;
using Newtonsoft.Json;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Palettes;

namespace PixiEditor.Models.ExternalServices;

internal static class NbpFetcher
{
    public const string NbpApiUrl = "https://api.nbp.pl/api/exchangerates/rates/A";
    
    public static async Task<decimal?> FetchExchangeRate(string currency)
    {
        try
        {
            using HttpClient client = new HttpClient();
            string url = @$"{NbpApiUrl}/{currency}?format=json";

            HttpResponseMessage response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<NbpResponse>(content);
                

                return obj?.Rates?.FirstOrDefault()?.Mid;
            }
        }
        catch (HttpRequestException)
        {
            NoticeDialog.Show("FAILED_DOWNLOAD_RATE", "ERROR");
            return null;
        }

        return null;
    }
}
