namespace PixiEditor.Models.DataHolders.Palettes
{
    public class PaletteList
    {
        public bool FetchedCorrectly { get; set; }
        public WpfObservableRangeCollection<Palette> Palettes { get; set; }
    }
}
