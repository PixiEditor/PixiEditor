using System.IO;
using System.Reflection;

namespace PixiEditor.Models.IO;
public static class Paths
{
    /// <summary>
    /// Path to {installation folder}/Data
    /// </summary>
    public static string DataFullPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data");
    /// <summary>
    /// Path to {installation folder}/Extensions
    /// </summary>
    public static string ExtensionsFullPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Extensions");
    /// <summary>
    /// Path to %localappdata%/PixiEditor/Palettes
    /// </summary>
    public static string PathToPalettesFolder { get; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor", "Autosave");
    /// <summary>
    /// Path to %temp%/PixiEditor/Autosave
    /// </summary>
    public static string PathToUnsavedFilesFolder { get; } = Path.Join(
        Path.GetTempPath(),
        "PixiEditor", "Autosave");
}
