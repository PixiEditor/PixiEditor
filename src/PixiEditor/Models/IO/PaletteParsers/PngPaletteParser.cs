using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;
using Color = PixiEditor.DrawingApi.Core.ColorsImpl.Color;

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
            PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            BitmapFrame frame = decoder.Frames[0];

            PaletteColor[] colors = ExtractFromBitmap(frame);

            PaletteFileData data = new(
                Path.GetFileNameWithoutExtension(path), colors);

            return data;
        });
    }

    private PaletteColor[] ExtractFromBitmap(BitmapFrame frame)
    {
        if (frame.Palette is not null && frame.Palette.Colors.Count > 0)
        {
            return ExtractFromBitmapPalette(frame.Palette);
        }

        return ExtractFromBitmapSource(frame);
    }

    private PaletteColor[] ExtractFromBitmapSource(BitmapFrame frame)
    {
        if (frame.PixelWidth == 0 || frame.PixelHeight == 0)
        {
            return Array.Empty<PaletteColor>();
        }

        List<PaletteColor> colors = new();

        byte[] pixels = new byte[frame.PixelWidth * frame.PixelHeight * 4];
        frame.CopyPixels(pixels, frame.PixelWidth * 4, 0);
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

    private PaletteColor[] ExtractFromBitmapPalette(BitmapPalette palette)
    {
        if (palette.Colors == null || palette.Colors.Count == 0)
        {
            return Array.Empty<PaletteColor>();
        }

        return palette.Colors.Select(color => new PaletteColor(color.R, color.G, color.B)).ToArray();
    }

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
            WriteableBitmap bitmap = new(data.Colors.Length, 1, 96, 96, PixelFormats.Bgra32, null);
            bitmap.Lock();
            for (int i = 0; i < data.Colors.Length; i++)
            {
                PaletteColor color = data.Colors[i];
                bitmap.SetPixel(i, 0, new System.Windows.Media.Color { R = color.R, G = color.G, B = color.B, A = 255 });
            }

            bitmap.Unlock();
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using FileStream stream = new(path, FileMode.Create);
            encoder.Save(stream);
        });
    }
}
