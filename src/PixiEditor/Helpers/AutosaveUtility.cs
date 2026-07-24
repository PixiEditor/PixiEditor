using PixiEditor.Models.IO;

namespace PixiEditor.Helpers;

internal class AutosaveHelper
{
    static readonly string LegacyAutosavesPath = Path.Join(Path.GetTempPath(), "PixiEditor", "Autosave");

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
        // TODO: This is pretty much temporary, once enough time passes, no check for legacy autosave should be needed.
        if (!File.Exists(path))
        {
            return Path.Join(LegacyAutosavesPath, $"autosave-{guid}.pixi");
        }

        return path;
    }

    public static string GetNewAutosavePath(Guid guid)
    {
        string path = Path.Join(Paths.PathToUnsavedFilesFolder, $"autosave-{guid}.pixi");
        return path;
    }
}
