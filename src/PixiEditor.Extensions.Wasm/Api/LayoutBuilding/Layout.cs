using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public sealed class Layout : SingleChildLayoutElement
{
    public Layout(ILayoutElement<NativeControl> body = null)
    {
        Child = body;
    }

    public override NativeControl Build()
    {
        NativeControl layout = new NativeControl("Layout");

        if (Child != null)
            layout.AddChild(Child.Build());
        return layout;
    }

}
