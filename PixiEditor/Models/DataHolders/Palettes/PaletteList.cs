using PixiEditor.Helpers;
using System.Collections.ObjectModel;

namespace PixiEditor.Models.DataHolders.Palettes
{
    public class PaletteList : NotifyableObject
    {
        public bool FetchedCorrectly { get; set; } = false;
        public WpfObservableRangeCollection<Palette> Palettes { get; set; } = new WpfObservableRangeCollection<Palette>();
    }
}
