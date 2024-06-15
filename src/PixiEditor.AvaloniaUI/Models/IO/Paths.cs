using System.IO;
using System.Reflection;

namespace PixiEditor.AvaloniaUI.Models.IO;
public static class Paths
{
    public static string DataResourceUri { get; } = $"avares://{typeof(Paths).Assembly.GetName().Name}/Data/";
    public static string DataFullPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data");
    public static string ExtensionPackagesPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Extensions");
    public static string UserExtensionsPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "PixiEditor", "Extensions");

    public static string PathToPalettesFolder { get; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PixiEditor", "Palettes");

    public static string InternalResourceDataPath { get; } =
        $"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Data";
    
    public static string TempRenderingPath { get; } = Path.Combine(Path.GetTempPath(), "PixiEditor", "Rendering");
}
