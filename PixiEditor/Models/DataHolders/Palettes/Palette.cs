using PixiEditor.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PixiEditor.Models.DataHolders.Palettes
{
    public class Palette
    {
        public string Title { get; set; }
        public List<string> Colors { get; set; }
        public string[] Tags { get; set; }

        public Palette(string title, List<string> colors, string[] tags)
        {
            Title = title;
            Colors = colors;
            Tags = tags;
        }
    }
}
