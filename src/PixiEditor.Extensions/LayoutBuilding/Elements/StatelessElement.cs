using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.State;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public abstract class StatelessElement : LayoutElement, IStatelessElement<Control>
{
    public override Control BuildNative()
    {
        return Build().BuildNative();
    }

    public virtual ILayoutElement<Control> Build()
    {
        return this;
    }
}
