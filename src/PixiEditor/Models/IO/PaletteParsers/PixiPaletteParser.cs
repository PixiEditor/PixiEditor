using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Parser;
using PixiEditor.Parser.Old.PixiV4;

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
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var file = await PixiParser.DeserializeUsingCompatibleAsync(fileStream);

        var palette = file switch
        {
            Document v5 => v5.Palette,
            DocumentV4 v4 => v4.Palette,
            _ => null
        };
        
        if(palette == null) return PaletteFileData.Corrupted;

        string name = Path.GetFileNameWithoutExtension(path);

        return new PaletteFileData(name, palette.Select(x => new PaletteColor(x.R, x.G, x.B)).ToArray());
    }

    public override bool CanSave => false;

    public override Task<bool> Save(string path, PaletteFileData data)
    {
        throw new SavingNotSupportedException("Saving palette as .pixi directly is not supported.");
    }
}
