using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Palettes;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class PalettePropertyViewModel : NodePropertyViewModel<Palette>
{
    public ObservableCollection<PaletteColorReference> Swatches { get; }

    public RelayCommand AddColorCommand { get; }

    public PalettePropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
        Swatches = new ObservableCollection<PaletteColorReference>();
        AddColorCommand = new RelayCommand(AddColor);
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Value))
            return;

        int requiredCount = Value?.Count ?? 0;

        if (Swatches.Count != requiredCount)
        {
            Swatches.Clear();
            for (int i = 0; i < requiredCount; i++)
            {
                Swatches.Add(new PaletteColorReference(this, i));
            }
        }

        OnPropertyChanged(nameof(Swatches));

        foreach (PaletteColorReference swatch in Swatches)
        {
            swatch.ColorChanged();
        }
    }

    private void AddColor()
    {
        List<Color> colors = CurrentColors();
        colors.Add(Colors.Black);
        PushColors(colors);
    }

    private void RemoveColor(int index)
    {
        List<Color> colors = CurrentColors();
        if (index < 0 || index >= colors.Count)
            return;

        colors.RemoveAt(index);
        PushColors(colors);
    }

    private List<Color> CurrentColors()
    {
        return Value?.ToList() ?? new List<Color>();
    }

    public void ImportColors(IEnumerable<Color> colors)
    {
        PushColors(colors);
    }

    private void PushColors(IEnumerable<Color> colors)
    {
        Palette newPalette = new Palette(colors);
        ViewModelMain.Current.NodeGraphManager.UpdatePropertyValue((Node, PropertyName, newPalette));
    }

    public class PaletteColorReference(PalettePropertyViewModel viewModel, int index) : PixiObservableObject
    {
        public RelayCommand RemoveCommand { get; } = new RelayCommand(() => viewModel.RemoveColor(index));

        public Color Color
        {
            get => viewModel.Value != null && index < viewModel.Value.Count ? viewModel.Value[index] : Colors.Black;
            set
            {
                List<Color> colors = viewModel.CurrentColors();
                if (index < 0 || index >= colors.Count)
                    return;

                colors[index] = value;
                viewModel.PushColors(colors);
                OnPropertyChanged();
            }
        }

        public void ColorChanged()
        {
            OnPropertyChanged(nameof(Color));
        }
    }
}
