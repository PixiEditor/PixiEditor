using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm;

internal class Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void LogMessage(string message);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void CreatePopupWindow(string title, IntPtr data, int length);

    internal static void Load()
    {
        Type extensionType = Assembly.GetEntryAssembly().ExportedTypes
            .FirstOrDefault(type => type.IsSubclassOf(typeof(WasmExtension)));

        Debug.Assert(extensionType != null, "extensionType != null");

        LogMessage($"Loading extension {extensionType.FullName}");

        WasmExtension extension = (WasmExtension)Activator.CreateInstance(extensionType);
        ExtensionContext.Active = extension;
        extension.OnLoaded();
    }

    internal static void Initialize()
    {
        ExtensionContext.Active.OnInitialized();
    }

    internal static void EventRaised(int internalControlId, string eventName) //TOOD: Args
    {
        WasmExtension.Api.Logger.Log($"Event raised: {eventName} on {internalControlId}");
        if (LayoutElementsStore.LayoutElements.TryGetValue(internalControlId, out ILayoutElement<NativeControl> element))
        {
            element.RaiseEvent(eventName, new ElementEventArgs());
        }
    }
}
