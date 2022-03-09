using System.Collections.ObjectModel;

namespace PixiEditor.Models.DataHolders.Palettes
{
    public class Palette
    {
        public string Title { get; set; }
        public WpfObservableRangeCollection<string> Colors { get; set; }
        public string[] Tags { get; set; }

        public Palette() { }
        public Palette(string title, WpfObservableRangeCollection<string> colors, string[] tags)
        {
            Title = title;
            Colors = colors;
            Tags = tags;
        }
    }
}
