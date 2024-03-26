using PixiEditor.Extensions.Wasm.Api;
using PixiEditor.Extensions.Wasm.Api.Logging;
using PixiEditor.Extensions.Wasm.Api.Window;

namespace PixiEditor.Extensions.Wasm;

public class PixiEditorApi
{
    public ILogger Logger { get; }
    public IWindowProvider WindowProvider { get; }

    public PixiEditorApi()
    {
        Logger = new Logger();
        WindowProvider = new WindowProvider();
    }
}
