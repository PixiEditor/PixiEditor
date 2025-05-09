using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Container : SingleChildLayoutElement
{
    public Edges Margin { get; set; }
    public Color BackgroundColor { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }

    public Container(LayoutElement child = null, Edges margin = default, Color backgroundColor = default, double width = -1, double height = -1, Cursor? cursor = null) : base(cursor)
    {
        Margin = margin;
        BackgroundColor = backgroundColor;
        Width = width;
        Height = height;
        Child = child;
    }
    
    protected override ControlDefinition CreateControl()
    {
        ControlDefinition container = new ControlDefinition(UniqueId, "Container");
        container.AddProperty(Margin);

        container.AddProperty(BackgroundColor);
        
        container.AddProperty(Width);
        container.AddProperty(Height);

        if(Child != null)
        {
            container.AddChild(Child.BuildNative());
        }

        return container;
    }
}
