namespace PixiEditor.Extensions.CommonApi.Palettes.Parsers;

public abstract class PaletteFileParser
{
    public abstract Task<PaletteFileData> Parse(string path);
    public abstract Task<bool> Save(string path, PaletteFileData data);
    public abstract string FileName { get; }
    public abstract string[] SupportedFileExtensions { get; }

    public virtual bool CanSave => true;

    protected static async Task<string[]> ReadTextLines(string path)
    {
        using var stream = File.OpenText(path);
        string fileContent = await stream.ReadToEndAsync();
        string[] lines = fileContent.Split('\n');
        return lines;
    }

    public override string ToString()
    {
        return FileName;
    }
}
