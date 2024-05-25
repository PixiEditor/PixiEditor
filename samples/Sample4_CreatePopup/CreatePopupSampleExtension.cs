using PixiEditor.Extensions.Wasm;
using PixiEditor.Extensions.Wasm.Api.FlyUI;

namespace CreatePopupSample;

public class CreatePopupSampleExtension : WasmExtension
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
    public override async void OnInitialized()
    {
        var popup = Api.WindowProvider.CreatePopupWindow("Hello World", new Text("Hello from popup!"));
        await popup.ShowDialog().;
        Api.Logger.Log("Popup closed");
    }
}