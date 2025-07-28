using System.IO;
using PixiEditor.Extensions.Sdk;

namespace PalettesSample;

public class PalettesSampleExtension : PixiEditorExtension
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
        ExamplePaletteDataSource dataSource = new ExamplePaletteDataSource("Palettes Sample");
        Api.Palettes.RegisterDataSource(dataSource);
        
        Api.Logger.Log("Palettes registered!");
    }
}