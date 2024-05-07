using System.Runtime.InteropServices;
using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;
using PixiEditor.Extensions.Wasm.Utilities;

namespace PixiEditor.Extensions.Wasm.Api.Window;

public class WindowProvider : IWindowProvider
{
    public void CreatePopupWindow(string title, LayoutElement body)
    {
        CompiledControl compiledControl = body.BuildNative();
        byte[] bytes = compiledControl.Serialize().ToArray();
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Interop.CreatePopupWindow(title, ptr, bytes.Length);
        Marshal.FreeHGlobal(ptr);

        SubscribeToEvents(compiledControl);
    }

    void IWindowProvider.LayoutStateChanged(int uniqueId, CompiledControl newLayout)
    {
        byte[] bytes = newLayout.Serialize().ToArray();
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Interop.StateChanged(uniqueId, ptr, bytes.Length);
        Marshal.FreeHGlobal(ptr);

        SubscribeToEvents(newLayout);
    }

    private void SubscribeToEvents(CompiledControl body)
    {
        foreach (CompiledControl child in body.Children)
        {
            SubscribeToEvents(child);
        }

        foreach (var queuedEvent in body.QueuedEvents)
        {
            Interop.SubscribeToEvent(body.UniqueId, queuedEvent);
        }
    }
}
