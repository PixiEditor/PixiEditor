using System.IO;
using CLSEncoderDecoder;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.Models.IO.PaletteParsers;

internal class ClsFileParser : PaletteFileParser
{
    public override string FileName { get; } = "Clip Studio Paint Color Set";

    public override string[] SupportedFileExtensions { get; } = { ".cls" };

    public override async Task<PaletteFileData> Parse(string path)
    {
        return await Task.Run(() =>
        {
            ClsColorSet set;
            try
            {
                set = ClsColorSet.Load(path);
            }
            catch
            {
                return PaletteFileData.Corrupted;
            }

            PaletteFileData data = new(
                set.Utf8Name,
                set.Colors
                    .Where(static color => color.Alpha > 0)
                    .Select(static color => new Color(color.Red, color.Green, color.Blue, 255))
                    .ToArray()
            );
            return data;
        });
    }

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        if (data?.Colors == null || data.Colors.Length <= 0) return false;

        string name = data.Title;
        List<ClsColor> colors = data.Colors
            .Select(color => new ClsColor(color.R, color.G, color.B, color.A)).ToList();
        await Task.Run(() =>
        {
            if (name.Length == 0)
                name = Path.GetFileNameWithoutExtension(path);
            if (name.Length > 64)
                name = name.Substring(0, 64);
            ClsColorSet set = new(colors, name);
            set.Save(path);
        });

        return true;

    }

}
