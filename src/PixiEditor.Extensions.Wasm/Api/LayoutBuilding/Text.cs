namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Text(string value) : StatelessElement
{
    public string Value { get; set; } = value;

    public override CompiledControl BuildNative()
    {
        CompiledControl text = new CompiledControl(UniqueId, "Text");
        text.AddProperty(Value, typeof(string));

        BuildPendingEvents(text);
        return text;
    }
}
