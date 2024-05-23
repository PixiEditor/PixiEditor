using PixiEditor.Extensions.Wasm.Api.Localization;

namespace HelloWorld;

using PixiEditor.Extensions.Wasm;

public class LocalizationSampleExtension : WasmExtension
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
        Api.Logger.Log(new LocalizedString("LOC_SAM:HELLO_WORLD"));
        Api.Logger.Log(new LocalizedString("LOC_SAM:HELLO_WORLD_ARGS", "John Doe"));
    }
}