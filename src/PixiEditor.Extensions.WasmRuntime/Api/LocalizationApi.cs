using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class LocalizationApi : ApiGroupHandler
{
    [ApiFunction("translate_key")]
    public string TranslateKey(string key)
    {
        string finalKey = key;
        var split = key.Split(":");
        if (split.Length == 1)
        {
            finalKey = $"{Metadata.UniqueName}:{key}";
        }
        else if (split.Length == 2)
        {
            string caller = split[0];
            string keyName = split[1];
            if (caller.Equals("pixieditor", StringComparison.InvariantCultureIgnoreCase))
            {
                finalKey = keyName;
            }
        }
        
        LocalizedString localizedString = new LocalizedString(finalKey);
        return localizedString.Value;
    }
}
