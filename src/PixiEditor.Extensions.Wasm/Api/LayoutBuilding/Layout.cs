using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public sealed class Layout : ISingleChildLayoutElement<NativeControl>
{
    public ILayoutElement<NativeControl> Child { get; set; }

    public Layout(ILayoutElement<NativeControl> body = null)
    {
        Child = body;
    }

    public NativeControl Build()
    {
        NativeControl layout = new NativeControl("Layout");
        layout.AddChild(Child.Build());
        return layout;
    }

}
