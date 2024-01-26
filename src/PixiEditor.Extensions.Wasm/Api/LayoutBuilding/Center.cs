using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Center : SingleChildLayoutElement
{
    public Center(ILayoutElement<NativeControl> child)
    {
        Child = child;
    }

    public override NativeControl Build()
    {
        NativeControl center = new NativeControl("Center");

        if (Child != null)
            center.AddChild(Child.Build());
        return center;
    }
}
