using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PixiEditor.Models.DataHolders.Palettes;

internal sealed class PaletteList : ObservableObject
{
    public bool FetchedCorrectly { get; set; } = false;
    public ObservableCollection<Palette> Palettes { get; set; } = new ObservableCollection<Palette>();
}
