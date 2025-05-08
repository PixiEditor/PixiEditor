using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Border : SingleChildLayoutElement
{
    public Color Color { get; set; }
    public Edges Thickness { get; set; }

    public Edges CornerRadius { get; set; }

    public Edges Padding { get; set; }

    public Edges Margin { get; set; }

    public Color BackgroundColor { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public Border(LayoutElement child = null, Color color = default, Edges thickness = default,
        Edges cornerRadius = default, Edges padding = default, Edges margin = default, double width = -1,
        double height = -1,
        Color backgroundColor = default)
    {
        Child = child;
        Color = color;
        Thickness = thickness;
        CornerRadius = cornerRadius;
        Padding = padding;
        Margin = margin;
        Width = width;
        Height = height;
        BackgroundColor = backgroundColor;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl control = new(UniqueId, "Border");
        if (Child != null)
        {
            control.Children.Add(Child.BuildNative());
        }

        control.AddProperty(Color);
        control.AddProperty(Thickness);
        control.AddProperty(CornerRadius);
        control.AddProperty(Padding);
        control.AddProperty(Margin);
        control.AddProperty(Width);
        control.AddProperty(Height);
        control.AddProperty(BackgroundColor);

        return control;
    }
}
