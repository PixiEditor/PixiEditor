using System.Runtime.InteropServices;

namespace PixiEditor.Extensions.Wasm;

public abstract class WasmExtension
{
    public static PixiEditorApi Api { get; } = new PixiEditorApi();
    public virtual void OnLoaded() { }
    public virtual void OnInitialized() { }
}
