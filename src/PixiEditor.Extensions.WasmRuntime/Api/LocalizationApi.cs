using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class LocalizationApi : ApiGroupHandler
{
    [ApiFunction("translate_key")]
    public static string TranslateKey(string key)
    {
        LocalizedString localizedString = new LocalizedString(key);
        return localizedString.Value;
    }
}
