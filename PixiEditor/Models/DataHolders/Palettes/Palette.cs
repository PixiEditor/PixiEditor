namespace PixiEditor.Models.DataHolders.Palettes
{
    public class Palette
    {
        public string Title { get; set; }
        public ObservableCollection<string> Colors { get; set; }
        public string[] Tags { get; set; }

        public Palette() { }
        public Palette(string title, ObservableCollection<string> colors, string[] tags)
        {
            Title = title;
            Colors = colors;
            Tags = tags;
        }
    }
}
