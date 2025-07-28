using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Structures;

namespace PixiEditor.Models.Palettes;

internal sealed class PaletteList : ObservableObject
{
    public bool FetchedCorrectly { get; set; } = false;
    public ObservableRangeCollection<Palette> Palettes { get; set; } = new ObservableRangeCollection<Palette>();
}
