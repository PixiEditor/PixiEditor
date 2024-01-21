using PixiEditor.Extensions.Wasm.Api;

namespace PixiEditor.Extensions.Wasm;

public class PixiEditorApi
{
    public ILogger Logger { get; }

    public PixiEditorApi()
    {
        Logger = new Logger();
    }
}
