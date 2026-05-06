namespace PixiEditor.Models.ExternalServices;

internal static class GeoFetcher
{
    private static readonly Dictionary<string, string> CountryToCurrency = new()
    {
        {"PL", "PLN"}, {"US", "USD"}, {"DE", "EUR"}, {"FR", "EUR"}, {"IT", "EUR"},
        {"ES", "EUR"}, {"AT", "EUR"}, {"NL", "EUR"}, {"BE", "EUR"}, {"FI", "EUR"},
        {"PT", "EUR"}, {"IE", "EUR"}, {"LU", "EUR"}, {"MT", "EUR"}, {"GR", "EUR"},
        {"SI", "EUR"}, {"CY", "EUR"}, {"SK", "EUR"}, {"EE", "EUR"}, {"LV", "EUR"},
        {"LT", "EUR"}, {"CH", "CHF"}, {"GB", "GBP"}, {"JP", "JPY"}, {"NO", "NOK"},
        {"SE", "SEK"}, {"CZ", "CZK"}, {"DK", "DKK"}, {"CA", "CAD"}, {"AU", "AUD"}
    };
    
    private static readonly HashSet<string> UnsupportedCountries = new()
    {
        "CO", "ME", "KP", "KR", "IN", "RS", "MX", "AE", "TR", "AM"
    };
    
    public static async Task<string> GetUserCurrency()
    {
        try
        {
            using HttpClient client = new();
            var response = await client.GetStringAsync("https://ip2c.org/s");
            // Response format: "1;PL;POL;Poland"
            var parts = response.Split(';');
            if (parts.Length < 2)
                return "PLN"; // fallback

            string countryCode = parts[1];
            
            if (UnsupportedCountries.Contains(countryCode))
                return "UNSUPPORTED";

            return CountryToCurrency.TryGetValue(countryCode, out var currency) ? currency : "PLN";
        }
        catch
        {
            return "PLN"; // fallback
        }
    }
}
