using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Center : SingleChildLayoutElement
{
    public Center(ILayoutElement<CompiledControl> child)
    {
        Child = child;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl center = new CompiledControl(UniqueId, "Center");

        if (Child != null)
            center.AddChild(Child.BuildNative());

        BuildPendingEvents(center);
        return center;
    }
}
