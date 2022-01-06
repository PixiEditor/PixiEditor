using PixiEditor.Models.DataHolders.LospecPalette;
using SkiaSharp;
using System.Collections.Generic;

namespace PixiEditor.Models.DataHolders
{
    public class Palette
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Url => $"https://lospec.com/palette-list/{Slug}";
        public LospecUser User { get; set; }
        public ObservableCollection<string> Colors { get; set; }
        public int Likes { get; set; }
        public string[] Tags { get; set; }
    }
}
