using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Container : SingleChildLayoutElement
{
    public Edges Margin { get; set; }
    public Color BackgroundColor { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }

    public Container(LayoutElement child = null, Edges margin = default, Color backgroundColor = default, double width = -1, double height = -1)
    {
        Margin = margin;
        BackgroundColor = backgroundColor;
        Width = width;
        Height = height;
        Child = child;
    }
    
    public override ControlDefinition BuildNative()
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

        BuildPendingEvents(container);
        return container;
    }
}
