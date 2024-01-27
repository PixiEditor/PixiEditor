using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public sealed class Layout : SingleChildLayoutElement
{
    public Layout(ILayoutElement<CompiledControl> body = null)
    {
        Child = body;
    }

    public override CompiledControl Build()
    {
        CompiledControl layout = new CompiledControl(UniqueId, "Layout");

        if (Child != null)
            layout.AddChild(Child.Build());

        BuildPendingEvents(layout);
        return layout;
    }

}
