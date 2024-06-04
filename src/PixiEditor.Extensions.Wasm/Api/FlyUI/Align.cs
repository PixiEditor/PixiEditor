using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public class Align : SingleChildLayoutElement
{
    public Alignment Alignment { get; set; }

    public Align(Alignment alignment = Alignment.TopLeft, LayoutElement child = null)
    {
        Child = child;
        Alignment = alignment;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl control = new CompiledControl(UniqueId, "Align");
        control.AddProperty((int)Alignment);
        
        if (Child != null)
            control.AddChild(Child.BuildNative());

        BuildPendingEvents(control);
        return control;
    }
}
