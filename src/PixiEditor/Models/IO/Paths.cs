using System.IO;
using System.Reflection;

namespace PixiEditor.Models.IO;
public static class Paths
{
    public static string DataFullPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data");
    public static string ExtensionsFullPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Extensions");
    public static string PathToPalettesFolder { get; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor", "Palettes");
}
