using PixiEditor.Models.DataHolders.LospecPalette;
using SkiaSharp;
using System.Collections.Generic;

namespace PixiEditor.Models.DataHolders
{
    public class Palette
    {
        public string Title { get; set; }
        public LospecUser User { get; set; }
        public ObservableCollection<string> Colors { get; set; }
    }
}
