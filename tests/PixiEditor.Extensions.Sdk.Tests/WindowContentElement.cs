using System.Diagnostics.CodeAnalysis;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.Sdk.Api.FlyUI;

namespace PixiEditor.Extensions.Sdk.Tests;

[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "FlyUI style")]
public class WindowContentElement : StatelessElement
{
    public override ILayoutElement<ControlDefinition> Build()
    {
        Layout layout = new Layout();
        return layout;
    }
}