using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.State;

namespace PixiEditor.Extensions.FlyUI.Elements;

public abstract class StatelessElement : LayoutElement, IStatelessElement<Control>
{
    public override Control BuildNative()
    {
        return CreateNativeControl();
    }

    protected override Control CreateNativeControl()
    {
        return Build().BuildNative();
    }

    public virtual ILayoutElement<Control> Build()
    {
        return this;
    }
}
