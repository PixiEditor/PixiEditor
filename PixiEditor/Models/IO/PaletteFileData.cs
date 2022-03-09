using SkiaSharp;
using System;
using System.Collections.Generic;

namespace PixiEditor.Models.IO
{
    public class PaletteFileData
    {
        public string Title { get; set; }
        public SKColor[] Colors { get; set; }
        public string[] Tags { get; set; }

        public PaletteFileData(SKColor[] colors)
        {
            Colors = colors;
            Title = "";
            Tags = Array.Empty<string>();
        }

        public PaletteFileData(string title, SKColor[] colors, string[] tags)
        {
            Title = title;
            Colors = colors;
            Tags = tags;
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
}
