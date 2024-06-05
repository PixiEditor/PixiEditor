using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public class Border : SingleChildLayoutElement
{
    public Color Color { get; set; }
    public Edges Thickness { get; set; }
    
    public Edges CornerRadius { get; set; }
    
    public Edges Padding { get; set; }
    
    public Edges Margin { get; set; }

    public Border(LayoutElement child = null, Color color = default, Edges thickness = default, Edges cornerRadius = default, Edges padding = default, Edges margin = default)
    {
        Child = child;
        Color = color;
        Thickness = thickness;
        CornerRadius = cornerRadius;
        Padding = padding;
        Margin = margin;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl control = new(UniqueId, "Border");
        control.Children.Add(Child.BuildNative());

        control.AddProperty(Color);
        control.AddProperty(Thickness);
        control.AddProperty(CornerRadius);
        control.AddProperty(Padding);
        control.AddProperty(Margin);

        return control;
    }
}
