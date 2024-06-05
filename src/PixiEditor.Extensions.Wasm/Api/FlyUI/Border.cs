using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public class Border : SingleChildLayoutElement
{
    public Color Color { get; set; }
    public Edges Edges { get; set; }

    public Border(LayoutElement child = null, Color color = default, Edges edges = default)
    {
        Child = child;
        Color = color;
        Edges = edges;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl control = new(UniqueId, "Border");
        control.Children.Add(Child.BuildNative());

        control.AddProperty(Color);
        control.AddProperty(Edges);

        return control;
    }
}
