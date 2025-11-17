using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Brush = PixiEditor.Models.BrushEngine.Brush;

namespace PixiEditor.Views.Input;

internal partial class BrushPicker : UserControl
{
    public static readonly StyledProperty<ObservableCollection<Brush>> BrushesProperty =
        AvaloniaProperty.Register<BrushPicker, ObservableCollection<Brush>>(
            nameof(Brushes));

    public ObservableCollection<Brush> Brushes
    {
        get => GetValue(BrushesProperty);
        set => SetValue(BrushesProperty, value);
    }

    public static readonly StyledProperty<Brush?> SelectedBrushProperty =
        AvaloniaProperty.Register<BrushPicker, Brush?>(
            nameof(SelectedBrush));

    public Brush? SelectedBrush
    {
        get => GetValue(SelectedBrushProperty);
        set => SetValue(SelectedBrushProperty, value);
    }

    static BrushPicker()
    {
        BrushesProperty.Changed.AddClassHandler<BrushPicker>((x, e) =>
        {
            if (x.SelectedBrush == null && x.Brushes.Count > 0)
            {
                x.SelectedBrush = x.Brushes[0];
            }
        });
    }

    public BrushPicker()
    {
        InitializeComponent();
    }

    [RelayCommand]
    public void SelectBrush(Brush brush)
    {
        SelectedBrush = brush;
        PopupToggle.IsChecked = false;
        PopupToggle.Flyout.Hide();
    }
}
