using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Column : MultiChildLayoutElement
{
    public MainAxisAlignment MainAxisAlignment { get; set; }
    public CrossAxisAlignment CrossAxisAlignment { get; set; }

    public Column(
        MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Start,
        LayoutElement[] children = null, Cursor? cursor = null) : base(cursor)
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

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition controlDefinition = new ControlDefinition(UniqueId, "Column");
        controlDefinition.AddProperty(MainAxisAlignment);
        controlDefinition.AddProperty(CrossAxisAlignment);
        controlDefinition.Children.AddRange(Children.Where(x => x != null).Select(x => x.BuildNative()));

        return controlDefinition;
    }
}
