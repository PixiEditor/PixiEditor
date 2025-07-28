using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Api.Localization;

namespace LocalizationSample;

public class LocalizationSampleExtension : PixiEditorExtension
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
        // You can either use direct key or ExtensionUniqueName:Key to access localization strings.
        Api.Logger.Log(new LocalizedString("HELLO_WORLD"));
        Api.Logger.Log(new LocalizedString("HELLO_NAME", "John Doe"));

        // By prepending "PixiEditor:" to the key, you can access built-in PixiEditor localization strings.
        // if you prepend any other extension unique name, you can access that extension localization strings.
        Api.Logger.Log(new LocalizedString("PixiEditor:SHOW_IMAGE_PREVIEW_TASKBAR"));
    }
}