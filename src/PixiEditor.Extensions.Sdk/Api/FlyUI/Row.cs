namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Row : MultiChildLayoutElement
{
    public MainAxisAlignment MainAxisAlignment { get; set; }
    public CrossAxisAlignment CrossAxisAlignment { get; set; }

    public Row(params LayoutElement[] children)
    {
        Children = new List<LayoutElement>(children);
        MainAxisAlignment = MainAxisAlignment.Start;
        CrossAxisAlignment = CrossAxisAlignment.Start;
    }

    public Row(
        MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Start,
        LayoutElement[] children = null)
    {
        MainAxisAlignment = mainAxisAlignment;
        CrossAxisAlignment = crossAxisAlignment;
        Children = new List<LayoutElement>(children);
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl control = new CompiledControl(UniqueId, "Row");
        control.AddProperty(MainAxisAlignment);
        control.AddProperty(CrossAxisAlignment);
        control.Children.AddRange(Children.Select(x => x.BuildNative()));

        return control;
    }
}
