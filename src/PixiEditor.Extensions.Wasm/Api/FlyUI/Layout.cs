using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public sealed class Layout : SingleChildLayoutElement
{
    public Layout(ILayoutElement<CompiledControl> body = null)
    {
        Child = body;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl layout = new CompiledControl(UniqueId, "Layout");

        if (Child != null)
            layout.AddChild(Child.BuildNative());

        BuildPendingEvents(layout);
        return layout;
    }

}
