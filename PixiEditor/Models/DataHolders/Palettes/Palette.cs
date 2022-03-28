#nullable enable
using PixiEditor.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace PixiEditor.Models.DataHolders.Palettes
{
    public class Palette
    {
        public string Name { get; set; }
        public List<string> Colors { get; set; }

        private string? fileName;

        public string? FileName
        {
            get => fileName;
            set => fileName = ReplaceInvalidChars(value);
        }

        public bool IsFavourite { get; set; }

        public Palette(string name, List<string> colors, string fileName)
        {
            Name = name;
            Colors = colors;
            FileName = fileName;
        }

        private string? ReplaceInvalidChars(string? filename)
        {
            return filename == null ? null : string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
