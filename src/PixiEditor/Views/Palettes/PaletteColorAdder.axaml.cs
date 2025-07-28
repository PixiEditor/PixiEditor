using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Models.Structures;

namespace PixiEditor.Views.Palettes;

internal partial class PaletteColorAdder : UserControl
{
    public static readonly StyledProperty<Color> HintColorProperty =
        AvaloniaProperty.Register<PaletteColorAdder, Color>(
            nameof(HintColor),
            Avalonia.Media.Colors.Transparent);

    public Color HintColor
    {
        get => GetValue(HintColorProperty);
        set => SetValue(HintColorProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<PaletteColor>> SwatchesProperty =
        AvaloniaProperty.Register<PaletteColorAdder, ObservableCollection<PaletteColor>>(
            nameof(Swatches));

    public ObservableCollection<PaletteColor> Swatches
    {
        get => GetValue(SwatchesProperty);
        set => SetValue(SwatchesProperty, value);
    }

    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<PaletteColorAdder, Color>(
            nameof(SelectedColor),
            Avalonia.Media.Colors.Black);

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly StyledProperty<ObservableRangeCollection<PaletteColor>> ColorsProperty =
        AvaloniaProperty.Register<PaletteColorAdder, ObservableRangeCollection<PaletteColor>>(
            nameof(Colors));

    public ObservableRangeCollection<PaletteColor> Colors
    {
        get => GetValue(ColorsProperty);
        set => SetValue(ColorsProperty, value);
    }

    static PaletteColorAdder()
    {
        ColorsProperty.Changed.Subscribe(OnColorsChanged);
        SwatchesProperty.Changed.Subscribe(OnSwatchesChanged);
    }

    private void Colors_CollectionChanged(object sender,
        NotifyCollectionChangedEventArgs e)
    {
        UpdateAddSwatchesButton();
        UpdateAddButton();
    }

    private void UpdateAddButton()
    {
        if(Colors == null) return;
        if (SelectedColor == null) return;

        AddButton.IsEnabled = !Colors.Contains(ToPaletteColor(SelectedColor)) && SelectedColor.A == 255;
    }

    private static void OnColorsChanged(AvaloniaPropertyChangedEventArgs<ObservableRangeCollection<PaletteColor>> e)
    {
        PaletteColorAdder adder = (PaletteColorAdder)e.Sender;
        if (adder == null || adder.Colors == null) return;
        if (e.NewValue != null)
        {
            adder.UpdateAddButton();
            adder.Colors.CollectionChanged += adder.Colors_CollectionChanged;
        }
        else if (e.OldValue != null)
        {
            adder.Colors.CollectionChanged -= adder.Colors_CollectionChanged;
        }
    }

    private static void OnSwatchesChanged(AvaloniaPropertyChangedEventArgs<ObservableCollection<PaletteColor>> e)
    {
        PaletteColorAdder adder = (PaletteColorAdder)e.Sender;
        if (adder == null || adder.Swatches == null) return;
        if (e.NewValue != null)
        {
            adder.UpdateAddSwatchesButton();
            adder.Swatches.CollectionChanged += adder.Swatches_CollectionChanged;
        }
        else if (e.OldValue != null)
        {
            adder.Swatches.CollectionChanged -= adder.Swatches_CollectionChanged;
        }
    }

    private void Swatches_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateAddSwatchesButton();
    }

    private void UpdateAddSwatchesButton()
    {
        AddFromSwatches.IsEnabled = Swatches != null && Swatches.Any(x => !Colors.Contains(x));
    }

    public PaletteColorAdder()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        PaletteColor color = ToPaletteColor(SelectedColor);
        if (!Colors.Contains(color))
        {
            Colors.Add(color);
            AddButton.IsEnabled = false;
        }
    }

    private void PortableColorPicker_ColorChanged(object sender, RoutedEventArgs e) =>
        AddButton.IsEnabled = !Colors.Contains(ToPaletteColor(SelectedColor));

    private static PaletteColor ToPaletteColor(Color color) => new PaletteColor(color.R, color.G, color.B);

    private void AddFromSwatches_OnClick(object sender, RoutedEventArgs e)
    {
        if (Swatches == null) return;

        foreach (var color in Swatches)
        {
            if (!Colors.Contains(color))
            {
                Colors.Add(color);
            }
        }

        AddFromSwatches.IsEnabled = false;
    }
}
