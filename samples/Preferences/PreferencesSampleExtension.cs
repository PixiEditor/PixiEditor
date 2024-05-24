using PixiEditor.Extensions.Wasm;

namespace Preferences;

public class PreferencesSampleExtension : WasmExtension
{
    /// <summary>
    ///     This method is called when extension is loaded.
    ///  All extensions are first loaded and then initialized. This method is called before <see cref="OnInitialized"/>.
    /// </summary>
    public override void OnLoaded()
    {
    }

    /// <summary>
    ///     This method is called when extension is initialized. After this method is called, you can use Api property to access PixiEditor API.
    /// </summary>
    public override void OnInitialized()
    {
        // Internally this preference will have name "yourCompany.Samples.Preferences:HelloCount".
        int helloCount = Api.Preferences.GetPreference<int>("HelloCount");

        Api.Logger.Log($"Hello count: {helloCount}");

        Api.Preferences.UpdatePreference("HelloCount", helloCount + 1);

        // This will overwrite built-in PixiEditor preference. Extension must have WriteNonOwnedPreferences permission.
        // Prepending "PixiEditor:" to preference name will access built-in PixiEditor preferences. If you set it to other extension unique name,
        // it will access extension preferences.
        // You can do analogous thing with UpdatePreference.
        Api.Preferences.UpdateLocalPreference(
            "PixiEditor:OverwrittenPixiEditorPreference",
            "This is overwritten value of preference that is built-in in PixiEditor.");

        // You don't need any special permission for reading any kind of preference.
        Api.Logger.Log(Api.Preferences.GetLocalPreference<string>("PixiEditor:OverwrittenPixiEditorPreference"));
    }
}