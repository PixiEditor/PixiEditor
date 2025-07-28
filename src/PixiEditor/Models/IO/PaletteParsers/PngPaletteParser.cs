using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using PixiEditor.Helpers.Extensions;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Helpers;
using Drawie.Numerics;

namespace PixiEditor.Models.IO.PaletteParsers;

internal class PngPaletteParser : PaletteFileParser
{
    public override string FileName { get; } = "PNG Palette";
    public override string[] SupportedFileExtensions { get; } = { ".png" };

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
        return await Task.Run(() =>
        {
            Bitmap bitmap = new(path);

            PaletteColor[] colors = ExtractFromBitmap(bitmap);

            PaletteFileData data = new(
                Path.GetFileNameWithoutExtension(path), colors);

            return data;
        });
    }

    private PaletteColor[] ExtractFromBitmap(Bitmap bmp)
    {
        /*if (bmp.Palette is not null && bmp.Palette.Colors.Count > 0)
        {
            return ExtractFromBitmapPalette(bmp.Palette);
        }*/

        return ExtractFromBitmapSource(bmp);
    }

    private PaletteColor[] ExtractFromBitmapSource(Bitmap frame)
    {
        int width = frame.PixelSize.Width;
        int height = frame.PixelSize.Height;
        if (width == 0 || height == 0)
        {
            return Array.Empty<PaletteColor>();
        }

        List<PaletteColor> colors = new();

        byte[] pixels = frame.ExtractPixels();
        int pixelCount = pixels.Length / 4;
        for (int i = 0; i < pixelCount; i++)
        {
            var color = GetColorFromBytes(pixels, i);
            if (!colors.Contains(color))
            {
                colors.Add(color);
            }
        }

        return colors.ToArray();
    }

    private PaletteColor GetColorFromBytes(byte[] pixels, int i)
    {
        return new PaletteColor(pixels[i * 4 + 2], pixels[i * 4 + 1], pixels[i * 4]);
    }

    // TODO: there is no palette in Bitmap, maybe there is a different way to parse png
    /*private PaletteColor[] ExtractFromBitmapPalette(BitmapPalette palette)
    {
        if (palette.Colors == null || palette.Colors.Count == 0)
        {
            return Array.Empty<PaletteColor>();
        }

        return palette.Colors.Select(color => new PaletteColor(color.R, color.G, color.B)).ToArray();
    }*/

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        try
        {
            await SaveFile(path, data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task SaveFile(string path, PaletteFileData data)
    {
        await Task.Run(() =>
        {
            WriteableBitmap bitmap = WriteableBitmapUtility.CreateBitmap(new VecI(data.Colors.Length, 1));
            using var framebuffer = bitmap.Lock();
            for (int i = 0; i < data.Colors.Length; i++)
            {
                PaletteColor color = data.Colors[i];
                framebuffer.WritePixel(i, 0, new global::Avalonia.Media.Color(255, color.R, color.G, color.B));
            }

            bitmap.Save(path);
        });
    }
}
