using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Center : SingleChildLayoutElement
{
    public Center(ILayoutElement<CompiledControl> child)
    {
        Child = child;
    }

    public override CompiledControl Build()
    {
        CompiledControl center = new CompiledControl(UniqueId, "Center");

        if (Child != null)
            center.AddChild(Child.Build());

        BuildPendingEvents(center);
        return center;
    }
}
