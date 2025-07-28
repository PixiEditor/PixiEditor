using PixiEditor.Extensions.FlyUI.Elements;
using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.WasmRuntime.Management;
using Wasmtime;

namespace PixiEditor.Extensions.WasmRuntime.Api;

// This is a "dummy" class, all properties and methods are never actually used or set, it is used to tell code generators the implementation of the API
// Compiler will convert all functions with [ApiFunction] attribute to an actual WASM linker function
internal class ApiGroupHandler
{
    public ExtensionServices Api { get; }
    protected LayoutBuilder LayoutBuilder { get; }
    protected ObjectManager NativeObjectManager { get; }
    protected AsyncCallsManager AsyncHandleManager { get; }
    protected Instance? Instance { get; }
    protected WasmMemoryUtility WasmMemoryUtility { get; }
    protected ExtensionMetadata Metadata { get; }
    protected WasmExtensionInstance Extension { get; }
}
