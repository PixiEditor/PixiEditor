using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.Wasm.Api.FlyUI;

namespace PixiEditor.Extensions.Wasm;

internal class Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void LogMessage(string message);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void CreatePopupWindow(string title, IntPtr data, int length);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void SubscribeToEvent(int internalControlId, string eventName);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void StateChanged(int uniqueId, IntPtr data, int length);

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
        if (LayoutElementsStore.LayoutElements.TryGetValue((int)internalControlId, out ILayoutElement<CompiledControl> element))
        {
            element.RaiseEvent(eventName ?? "", new ElementEventArgs());
        }
    }
}
