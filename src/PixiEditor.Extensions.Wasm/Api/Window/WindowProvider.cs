using System.Runtime.InteropServices;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;
using PixiEditor.Extensions.Wasm.Utilities;

namespace PixiEditor.Extensions.Wasm.Api.Window;

public class WindowProvider : IWindowProvider
{
    public void CreatePopupWindow(string title, LayoutElement body)
    {
        CompiledControl compiledControl = body.Build();
        byte[] bytes = compiledControl.Serialize().ToArray();
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        Interop.CreatePopupWindow(title, ptr, bytes.Length);
        Marshal.FreeHGlobal(ptr);

        SubscribeToEvents(compiledControl);
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
