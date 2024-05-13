using PixiEditor.Extensions.Wasm.Api.FlyUI;

namespace WasmSampleExtension;

public class ButtonTextElement : StatefulElement<ButtonTextElementState>
{
    public override ButtonTextElementState CreateState()
    {
        return new();
    }
}
