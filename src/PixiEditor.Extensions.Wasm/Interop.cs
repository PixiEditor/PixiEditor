using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.Wasm.Api.FlyUI;

namespace PixiEditor.Extensions.Wasm;

internal static class Program { internal static void Main() { } } // Required for compilation

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", 
    Justification = "Interop is a special case, it's injected to C code and follows C naming conventions.")]
internal static partial class Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void log_message(string message);


    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe void subscribe_to_event(int internalControlId, string eventName);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void state_changed(int uniqueId, IntPtr data, int length);


    // No need for [ApiExport] since this is a part of built-in C interop file.
    internal static void Load()
    {
        Type extensionType = Assembly.GetEntryAssembly().ExportedTypes
            .FirstOrDefault(type => type.IsSubclassOf(typeof(WasmExtension)));

        Debug.Assert(extensionType != null, "extensionType != null");

        log_message($"Loading extension {extensionType.FullName}");

        WasmExtension extension = (WasmExtension)Activator.CreateInstance(extensionType);
        ExtensionContext.Active = extension;
        extension.OnLoaded();
    }

    // No need for [ApiExport] since this is a part of built-in C interop file.
    internal static void Initialize()
    {
        ExtensionContext.Active.OnInitialized();
    }

    [ApiExport("raise_element_event")]
    internal static void EventRaised(int internalControlId, string eventName) //TOOD: Args
    {
        if (LayoutElementsStore.LayoutElements.TryGetValue((int)internalControlId, out ILayoutElement<CompiledControl> element))
        {
            element.RaiseEvent(eventName ?? "", new ElementEventArgs());
        }
    }
}
