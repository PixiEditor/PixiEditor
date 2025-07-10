using System.Text;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.FlyUI.Elements;
using PixiEditor.Extensions.WasmRuntime.Api.Modules;
using PixiEditor.Extensions.Windowing;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class VisualTreeApi : ApiGroupHandler
{
    [ApiFunction("find_ui_element")]
    public byte[] FindUiElement(string name, int elementHandle)
    {
        var element = Api.VisualTree.FindElement<Control>(name);

        var module = Extension.GetModule<UiModule>();
        byte[] serialized = module.ToSerializedNativeElement(element, elementHandle);
        return serialized;
    }

    [ApiFunction("find_ui_element_in_popup")]
    public byte[] FindUiElement(string name, int popupHandle, int elementHandle)
    {
        var element =
            Api.VisualTree.FindElement<Control>(name, NativeObjectManager.GetObject<PopupWindow>(popupHandle));

        var module = Extension.GetModule<UiModule>();
        byte[] serialized = module.ToSerializedNativeElement(element, elementHandle);
        return serialized;
    }

    [ApiFunction("append_element_to_native_multi_child")]
    public void AppendElementToNativeMultiChild(int atIndex, int uniqueId, Span<byte> body)
    {
        if (LayoutBuilder.ManagedElements.TryGetValue(uniqueId, out var element))
        {
            if (element is IChildHost childHost)
            {
                var deserializedChild = LayoutBuilder.Deserialize(body, DuplicateResolutionTactic.ThrowException);
                childHost.AppendChild(atIndex, deserializedChild);
            }
        }
    }
}
