using PixiEditor.Helpers;

namespace PixiEditor.Models.DataHolders.Palettes
{
    public class PaletteList
    {
        public bool FetchedCorrectly { get; set; } = false;
        public WpfObservableRangeCollection<Palette> Palettes { get; set; } = new WpfObservableRangeCollection<Palette>();
    }
}
