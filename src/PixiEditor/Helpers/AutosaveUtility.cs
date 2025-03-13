using PixiEditor.Models.IO;

namespace PixiEditor.Helpers;

internal class AutosaveHelper
{
    public static Guid? GetAutosaveGuid(string? path)
    {
        if (path is null)
            return null;

        string guidString = Path.GetFileNameWithoutExtension(path)["autosave-".Length..];
        return Guid.Parse(guidString);
    }

    public static string GetAutosavePath(Guid guid)
    {
        return Path.Join(Paths.PathToUnsavedFilesFolder, $"autosave-{guid}.pixi");
    }
}
