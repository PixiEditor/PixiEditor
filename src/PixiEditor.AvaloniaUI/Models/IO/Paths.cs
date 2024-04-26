using System.IO;
using System.Reflection;

namespace PixiEditor.AvaloniaUI.Models.IO;
public static class Paths
{
    public static string DataResourceUri { get; } = $"avares://{typeof(Paths).Assembly.GetName().Name}/Data/";
    public static string DataFullPath { get; } = $"avares://{typeof(Paths).Assembly.GetName().Name}/Data/";

    public static string ExtensionsFullPath { get; } = $"avares://{typeof(Paths).Assembly.GetName().Name}/Data/";

    public static string PathToPalettesFolder { get; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor", "Palettes");

    public static string InternalResourceDataPath { get; } =
        $"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Data";
}
