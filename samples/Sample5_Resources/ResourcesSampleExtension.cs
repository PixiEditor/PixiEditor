using System.IO;
using PixiEditor.Extensions.Wasm;

namespace ResourcesSample;

public class ResourcesSampleExtension : WasmExtension
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
        // By default, you can't access any files from the file system, however you can access files from the Resources folder.
        // This folder contains files that you put in the Resources folder in the extension project.
        Api.Logger.Log(File.ReadAllText("Resources/ExampleFile.txt"));
    }
}