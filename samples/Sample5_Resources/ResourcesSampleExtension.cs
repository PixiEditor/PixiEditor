using System.IO;
using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Api.Resources;
using PixiEditor.Extensions.Sdk.Bridge;

namespace ResourcesSample;

public class ResourcesSampleExtension : PixiEditorExtension
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
        // You can use System.File calls to access files in the Resources folder.
        // However, if you want to access files that are encrypted, you should use the Resources methods.
        // Adding <EncryptResources>true</EncryptResources> to the .csproj file will encrypt the resources in the Resources folder.
        Api.Logger.Log(Resources.ReadAllText("Resources/ExampleFile.txt"));

        Api.Logger.Log("Writing to file...");

        Resources.WriteAllText("Resources/ExampleFile.txt", "Hello from extension!");

        Api.Logger.Log(Resources.ReadAllText("Resources/ExampleFile.txt"));
    }
}