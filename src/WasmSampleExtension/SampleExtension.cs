using PixiEditor.Extensions.Wasm;

namespace SampleExtension.WASM;

public class SampleExtension : WasmExtension
{
    public override void OnLoaded()
    {
        Api.Logger.Log("WASM SampleExtension loaded!");
    }


    public override void OnInitialized()
    {
        Api.Logger.Log("WASM SampleExtension initialized!");
    }
}
