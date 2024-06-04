using PixiEditor.Extensions.Wasm;

namespace FlyUISample;

public class FlyUiSampleExtension : WasmExtension
{
    public override void OnInitialized()
    {
        WindowContentElement content = new WindowContentElement();
        Api.WindowProvider.CreatePopupWindow("Sample Window", content).Show();
    }
}