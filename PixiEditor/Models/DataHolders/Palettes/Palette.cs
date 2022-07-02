#nullable enable
using PixiEditor.Helpers;
using System.Collections.Generic;
using System.IO;
namespace PixiEditor.Models.DataHolders.Palettes
{
    public class Palette : NotifyableObject
    {
        private string _name = "";

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public List<string> Colors { get; set; }

        private string? fileName;

        public string? FileName
        {
            get => fileName;
            set
            {
                fileName = ReplaceInvalidChars(value);
                RaisePropertyChanged(nameof(FileName));
            }
        }

        public bool IsFavourite { get; set; }

        public Palette(string name, List<string> colors, string fileName)
        {
            Name = name;
            Colors = colors;
            FileName = fileName;
        }

        public static string? ReplaceInvalidChars(string? filename)
        {
            return filename == null ? null : string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
