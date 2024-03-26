using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

namespace WasmSampleExtension;

public class ButtonTextElement : StatefulElement<ButtonTextElementState>
{
    public override ButtonTextElementState CreateState()
    {
        return new();
    }
}
