namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Expanded : SingleChildLayoutElement
{
    public int Flex { get; set; }

    public Expanded(LayoutElement child = null, int flex = 1)
    {
        Child = child;
        Flex = flex;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl control = new CompiledControl(UniqueId, "Expanded");
        control.AddProperty(Flex);

        if (Child != null)
            control.AddChild(Child.BuildNative());

        BuildPendingEvents(control);
        return control;
    }
}
