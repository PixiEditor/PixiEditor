using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.Sdk.Attributes;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

[ControlTypeId("NativeElement")]
public class NativeElement : LayoutElement
{
    public NativeElement(Cursor? cursor) : base(cursor)
    {
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition control = new ControlDefinition(UniqueId, GetType());
        return control;
    }
}
