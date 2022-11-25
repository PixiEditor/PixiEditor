namespace PixiEditor.Models.IO;

internal abstract class PaletteFileParser
{
    public abstract Task<PaletteFileData> Parse(string path);
    public abstract Task Save(string path, PaletteFileData data);
    public abstract string FileName { get; }
    public abstract string[] SupportedFileExtensions { get; }
}
