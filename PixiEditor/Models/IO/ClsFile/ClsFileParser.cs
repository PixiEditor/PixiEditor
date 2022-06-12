using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CLSEncoderDecoder;
using SkiaSharp;

namespace PixiEditor.Models.IO.ClsFile;

public class ClsFileParser : PaletteFileParser
{
    public override string FileName { get; } = "Clip Studio Paint Color Set";

    public override string[] SupportedFileExtensions { get; } = { ".cls" };
    
    public override Task<PaletteFileData> Parse(string path)
    {
        return Task.Run(() =>
        {
            ClsColorSet set;
            try
            {
                set = ClsColorSet.Load(path);
            }
            catch (Exception e)
            {
                throw new FormatException("The provided file is invalid", e);
            }
            PaletteFileData data = new(
                set.Utf8Name,
                set.Colors
                    .Where(static color => color.Alpha > 0)
                    .Select(static color => new SKColor(color.Red, color.Green, color.Blue, 255))
                    .ToArray()
            );
            return data;
        });
    }

    public override Task Save(string path, PaletteFileData data)
    {   
        string name = data.Title;
        List<ClsColor> colors = data.Colors.Select(color => new ClsColor(color.Red, color.Green, color.Blue, color.Alpha)).ToList();
        return Task.Run(() =>
        {
            if (name.Length == 0)
                name = Path.GetFileNameWithoutExtension(path);
            if (name.Length > 64)
                name = name.Substring(0, 64);
            ClsColorSet set = new(colors, name); 
            set.Save(path);
        });
    }

}