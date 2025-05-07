namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Column : MultiChildLayoutElement
{
    public MainAxisAlignment MainAxisAlignment { get; set; }
    public CrossAxisAlignment CrossAxisAlignment { get; set; }

    public Column(
        MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Start,
        LayoutElement[] children = null)
    {
        MainAxisAlignment = mainAxisAlignment;
        CrossAxisAlignment = crossAxisAlignment;
        Children = new List<LayoutElement>(children);
    }

    public Column(params LayoutElement[] children)
    {
        MainAxisAlignment = MainAxisAlignment.Start;
        CrossAxisAlignment = CrossAxisAlignment.Start;
        Children = new List<LayoutElement>(children);
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl control = new CompiledControl(UniqueId, "Column");
        control.AddProperty(MainAxisAlignment);
        control.AddProperty(CrossAxisAlignment);
        control.Children.AddRange(Children.Where(x => x != null).Select(x => x.BuildNative()));

        return control;
    }
}
