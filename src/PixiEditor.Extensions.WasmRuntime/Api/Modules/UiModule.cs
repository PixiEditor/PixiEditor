using System.Text;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

internal class UiModule(WasmExtensionInstance extension) : ApiModule(extension)
{
    public LayoutBuilder LayoutBuilder => Extension.LayoutBuilder;

    public byte[] ToSerializedNativeElement(ILayoutElement<Control>? element, int elementHandle)
    {
        List<byte> result = new List<byte>();

        if (element is not null)
        {
            LayoutBuilder.ManagedElements.Add(elementHandle, element);

            var nativeElement = element.BuildNative();

            LayoutBuilder.ElementMap.ControlMapReversed.TryGetValue(element.BuildNative()?.GetType() ?? typeof(Control),
                out string? controlTypeId);

            if (controlTypeId is null)
            {
                if (nativeElement is Panel)
                {
                    controlTypeId = "MultiChildNativeElement";
                }
                else
                {
                    controlTypeId = "NativeElement";
                }
            }

            result.AddRange(BitConverter.GetBytes(controlTypeId.Length));
            result.AddRange(Encoding.UTF8.GetBytes(controlTypeId));
        }

        return result.ToArray();
    }
}
