using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Wasm.Api;
using PixiEditor.Extensions.Wasm.Api.Logging;
using PixiEditor.Extensions.Wasm.Api.Window;

namespace PixiEditor.Extensions.Wasm;

public class PixiEditorApi
{
    public Logger Logger { get; }
    public WindowProvider WindowProvider { get; }

    public PixiEditorApi()
    {
        Logger = new Logger();
        WindowProvider = new WindowProvider();
    }
}
