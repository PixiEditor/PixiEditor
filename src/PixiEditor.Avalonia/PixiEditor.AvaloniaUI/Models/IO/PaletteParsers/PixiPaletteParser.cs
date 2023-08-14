using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;
using PixiEditor.Models.IO;
using PixiEditor.Parser;

namespace PixiEditor.AvaloniaUI.Models.IO.PaletteParsers;

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

        return new PaletteFileData(name, file.Palette.Select(x => new PaletteColor(x.R, x.G, x.B)).ToArray());
    }

    public override bool CanSave => false;

    public override Task<bool> Save(string path, PaletteFileData data)
    {
        throw new SavingNotSupportedException("Saving palette as .pixi directly is not supported.");
    }
}
