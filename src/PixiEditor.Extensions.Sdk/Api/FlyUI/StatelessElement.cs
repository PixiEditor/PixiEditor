using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.State;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public abstract class StatelessElement : LayoutElement, IStatelessElement<ControlDefinition>
{
    public ILayoutElement<ControlDefinition> Build()
    {
        return this;
    }
}
