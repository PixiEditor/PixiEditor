using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PixiEditor.Models.IO;

public class PaletteFileData
{
    public string Title { get; set; }
    public SKColor[] Colors { get; set; }
    public bool IsCorrupted { get; set; } = false;

    public PaletteFileData(SKColor[] colors)
    {
        Colors = colors;
        Title = "";
    }

    public PaletteFileData(List<string> colors)
    {
        Colors = new SKColor[colors.Count];
        for (int i = 0; i < colors.Count; i++)
        {
            Colors[i] = SKColor.Parse(colors[i]);
        }

        Title = "";
    }

    public PaletteFileData(string title, SKColor[] colors)
    {
        Title = title;
        Colors = colors;
    }

    public string[] GetHexColors()
    {
        string[] colors = new string[Colors.Length];
        for (int i = 0; i < Colors.Length; i++)
        {
            colors[i] = Colors[i].ToString();
        }
        return colors;
    }
}