using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.Extensions.WasmRuntime;

internal class WindowingApi : ApiGroupHandler
{
    [ApiFunction("create_popup_window")]
    public int CreatePopupWindow(string title, Span<byte> bodySpan)
    {
        var body = LayoutBuilder.Deserialize(bodySpan, DuplicateResolutionTactic.ThrowException);
        var popupWindow = Api.Windowing.CreatePopupWindow(title, body.BuildNative());

        int handle = NativeObjectManager.AddObject(popupWindow);
        return handle;
    }
}
