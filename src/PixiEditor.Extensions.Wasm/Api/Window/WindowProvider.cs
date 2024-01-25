using System.Runtime.InteropServices;
using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;
using PixiEditor.Extensions.Wasm.Utilities;

namespace PixiEditor.Extensions.Wasm.Api.Window;

public class WindowProvider : IWindowProvider
{
    public void CreatePopupWindow(string title, NativeControl body)
    {
        byte[] bytes = body.Serialize().ToArray();
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Interop.CreatePopupWindow(title, ptr, bytes.Length);
    }
}
