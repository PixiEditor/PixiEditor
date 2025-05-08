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

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition controlDefinition = new(UniqueId, "Border");
        if (Child != null)
        {
            controlDefinition.Children.Add(Child.BuildNative());
        }

        controlDefinition.AddProperty(Color);
        controlDefinition.AddProperty(Thickness);
        controlDefinition.AddProperty(CornerRadius);
        controlDefinition.AddProperty(Padding);
        controlDefinition.AddProperty(Margin);
        controlDefinition.AddProperty(Width);
        controlDefinition.AddProperty(Height);
        controlDefinition.AddProperty(BackgroundColor);

        return controlDefinition;
    }
}
