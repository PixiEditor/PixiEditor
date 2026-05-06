using PixiEditor.Extensions.Sdk.Attributes;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

[ControlTypeId("Wrap")]
public class Wrap : MultiChildLayoutElement
{
    public double Spacing { get; set; }
    public double RunSpacing { get; set; }
    public ItemAlignment Alignment { get; set; }
    public Axis Direction { get; set; } = Axis.Horizontal;

    public Wrap(params LayoutElement[] children)
    {
        Children = new List<LayoutElement>(children);
    }

    public Wrap(double spacing = 0, double runSpacing = 0, ItemAlignment alignment = ItemAlignment.Start, Axis direction = Axis.Horizontal, params LayoutElement[] children)
    {
        Spacing = spacing;
        RunSpacing = runSpacing;
        Alignment = alignment;
        Direction = direction;
        Children = new List<LayoutElement>(children);
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition controlDefinition = new ControlDefinition(UniqueId, GetType());
        controlDefinition.AddProperty(Alignment);
        controlDefinition.AddProperty(Direction);
        controlDefinition.AddProperty(RunSpacing);
        controlDefinition.AddProperty(Spacing);
        controlDefinition.Children.AddRange(Children.Select(x => x.BuildNative()));

        return controlDefinition;
    }
}
