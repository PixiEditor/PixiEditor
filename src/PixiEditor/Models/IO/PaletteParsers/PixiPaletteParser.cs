using System.IO;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Parser;

namespace PixiEditor.Models.IO.PaletteParsers;

internal class PixiPaletteParser : PaletteFileParser
{
    public override string FileName { get; } = "Palette from PixiEditor .pixi";
    public override string[] SupportedFileExtensions { get; } = { ".pixi" };
    public override async Task<PaletteFileData> Parse(string path)
    {
        try
        {
            return await ParseFile(path);
        }
        catch
        {
            return PaletteFileData.Corrupted;
        }
    }

    private async Task<PaletteFileData> ParseFile(string path)
    {
        var file = await PixiParser.DeserializeAsync(path);
        if(file.Palette == null) return PaletteFileData.Corrupted;

        string name = Path.GetFileNameWithoutExtension(path);

        return new PaletteFileData(name, file.Palette.Select(x => new Color(x.R, x.G, x.B, x.A)).ToArray());
    }

    public override bool CanSave => false;

    public override Task<bool> Save(string path, PaletteFileData data)
    {
        throw new NotImplementedException("Saving palette to .pixi directly is not supported.");
    }
}
