using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Text : TextElement
{
    public Text(string value)
    {
        Value = value;
    }

    public override CompiledControl Build()
    {
        CompiledControl text = new CompiledControl(UniqueId, "Text");
        text.AddProperty(Value);

        BuildPendingEvents(text);
        return text;
    }
}
