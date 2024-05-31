using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Wasm.Api;
using PixiEditor.Extensions.Wasm.Api.Logging;
using PixiEditor.Extensions.Wasm.Api.Palettes;
using PixiEditor.Extensions.Wasm.Api.UserPreferences;
using PixiEditor.Extensions.Wasm.Api.Window;

namespace PixiEditor.Extensions.Wasm;

public class PixiEditorApi
{
    public Logger Logger { get; }
    public WindowProvider WindowProvider { get; }
    public Preferences Preferences { get; }
    public PalettesProvider Palettes { get; }

    public PixiEditorApi()
    {
        Logger = new Logger();
        WindowProvider = new WindowProvider();
        Preferences = new Preferences();
        Palettes = new PalettesProvider();
    }
}
