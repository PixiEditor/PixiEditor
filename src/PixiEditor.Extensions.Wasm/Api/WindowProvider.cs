namespace PixiEditor.Extensions.Wasm.Api;

public class WindowProvider : IWindowProvider
{
    public void CreatePopupWindow(string title, string body)
    {
        Interop.CreatePopupWindow(title, body);
    }
}
