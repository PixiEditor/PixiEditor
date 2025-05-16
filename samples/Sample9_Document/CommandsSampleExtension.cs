using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.Sdk;

namespace Sample9_Commands;

public class CommandsSampleExtension : PixiEditorExtension
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
        var doc = Api.Documents.ImportFile("Resources/cs.png", true); // Open file from the extension resources
        doc?.Resize(128, 128); // Resizes whole document
    }
}