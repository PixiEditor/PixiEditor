using PixiEditor.Models.IO;

namespace PixiEditor.Helpers;

internal class AutosaveHelper
{
    static string legacyAutosavePath = Path.Join(Path.GetTempPath(), "PixiEditor", "Autosave");

    public static Guid? GetAutosaveGuid(string? path)
    {
        if (path is null)
            return null;

        string guidString = Path.GetFileNameWithoutExtension(path)["autosave-".Length..];
        return Guid.Parse(guidString);
    }

    public static string GetAutosavePath(Guid guid)
    {

        string path = Path.Join(Paths.PathToUnsavedFilesFolder, $"autosave-{guid}.pixi");
        if (!File.Exists(path))
        {
            return Path.Join(legacyAutosavePath, $"autosave-{guid}.zip");
        }

        return path;
    }
}
