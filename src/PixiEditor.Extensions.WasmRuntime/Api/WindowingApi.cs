using PixiEditor.Extensions.FlyUI.Elements;
using PixiEditor.Extensions.WasmRuntime.Utilities;
using PixiEditor.Extensions.Windowing;

namespace PixiEditor.Extensions.WasmRuntime.Api;

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

    [ApiFunction("set_window_title")]
    public void SetWindowTitle(int handle, string title)
    {
        var window = NativeObjectManager.GetObject<PopupWindow>(handle);
        window.Title = title;
    }

    [ApiFunction("get_window_title")]
    public string GetWindowTitle(int handle)
    {
        var window = NativeObjectManager.GetObject<PopupWindow>(handle);
        return window.Title;
    }

    [ApiFunction("show_window")]
    public void ShowWindow(int handle)
    {
        var window = NativeObjectManager.GetObject<PopupWindow>(handle);
        window.Show();
    }
    
    [ApiFunction("show_window_async")]
    public int ShowWindowAsync(int handle)
    {
        var window = NativeObjectManager.GetObject<PopupWindow>(handle);
        Task<int> showDialogTask = AsyncUtility.ToResultFrom(window.ShowDialog());
        return AsyncHandleManager.AddAsyncCall(showDialogTask);
    }


    [ApiFunction("close_window")]
    public void CloseWindow(int handle)
    {
        var window = NativeObjectManager.GetObject<PopupWindow>(handle);
        window.Close();
    }
}
