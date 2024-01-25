using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Center : ISingleChildLayoutElement<NativeControl>
{
    public ILayoutElement<NativeControl> Child { get; set; }

    public Center(ILayoutElement<NativeControl> child)
    {
        Child = child;
    }

    NativeControl ILayoutElement<NativeControl>.Build()
    {
        NativeControl center = new NativeControl("Center");
        center.AddChild(Child.Build());
        return center;
    }
}
