using System.Runtime.CompilerServices;

using System.Runtime.CompilerServices;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Sdk.Api;
using PixiEditor.Extensions.Sdk.Api.Brushes;
using PixiEditor.Extensions.Sdk.Api.Commands;
using PixiEditor.Extensions.Sdk.Api.IO;
using PixiEditor.Extensions.Sdk.Api.Logging;
using PixiEditor.Extensions.Sdk.Api.Palettes;
using PixiEditor.Extensions.Sdk.Api.Tools;
using PixiEditor.Extensions.Sdk.Api.Ui;
using PixiEditor.Extensions.Sdk.Api.UserData;
using PixiEditor.Extensions.Sdk.Api.UserPreferences;
using PixiEditor.Extensions.Sdk.Api.Window;

[assembly: InternalsVisibleTo("PixiEditor.Extensions.Sdk.Tests")]
namespace PixiEditor.Extensions.Sdk;

public class PixiEditorApi
{
    public Logger Logger { get; }
    public ToolsProvider ToolsProvider { get; }
    public WindowProvider WindowProvider { get; }
    public Preferences Preferences { get; }
    public PalettesProvider Palettes { get; }
    public CommandProvider Commands { get; }
    public DocumentProvider Documents { get; }
    public VisualTreeProvider VisualTreeProvider { get; }
    public UserDataProvider UserDataProvider { get; }
    public BrushesProvider BrushesProvider { get; }

    public PixiEditorApi()
    {
        Logger = new Logger();
        WindowProvider = new WindowProvider();
        Preferences = new Preferences();
        Palettes = new PalettesProvider();
        Commands = new CommandProvider();
        Documents = new DocumentProvider();
        VisualTreeProvider = new VisualTreeProvider();
        UserDataProvider = new UserDataProvider();
        ToolsProvider = new ToolsProvider();
        BrushesProvider = new BrushesProvider();
    }
}
