using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using SkiaSharp;

namespace PixiEditor.Views.UserControls.Palettes;

/// <summary>
/// Interaction logic for PaletteColorAdder.xaml
/// </summary>
internal partial class PaletteColorAdder : UserControl
{
    public WpfObservableRangeCollection<SKColor> Colors
    {
        get { return (WpfObservableRangeCollection<SKColor>)GetValue(ColorsProperty); }
        set { SetValue(ColorsProperty, value); }
    }

    public Color HintColor
    {
        get { return (Color)GetValue(HintColorProperty); }
        set { SetValue(HintColorProperty, value); }
    }


    public static readonly DependencyProperty HintColorProperty =
        DependencyProperty.Register(nameof(HintColor), typeof(Color), typeof(PaletteColorAdder), new PropertyMetadata(System.Windows.Media.Colors.Transparent, OnHintColorChanged));

    private static void OnHintColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var adder = (PaletteColorAdder)d;
        Color newColor = (Color)e.NewValue;
        if (newColor.A < 255)
        {
            adder.HintColor = Color.FromArgb(255, newColor.R, newColor.G, newColor.B);
        }
    }

    public static readonly DependencyProperty SwatchesProperty = DependencyProperty.Register(nameof(Swatches), typeof(WpfObservableRangeCollection<SKColor>), typeof(PaletteColorAdder), new PropertyMetadata(default(WpfObservableRangeCollection<SKColor>), OnSwatchesChanged));

    public WpfObservableRangeCollection<SKColor> Swatches
    {
        get { return (WpfObservableRangeCollection<SKColor>)GetValue(SwatchesProperty); }
        set { SetValue(SwatchesProperty, value); }
    }

    public Color SelectedColor
    {
        get { return (Color)GetValue(SelectedColorProperty); }
        set { SetValue(SelectedColorProperty, value); }
    }


    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(PaletteColorAdder),
            new PropertyMetadata(System.Windows.Media.Colors.Black));



    public static readonly DependencyProperty ColorsProperty =
        DependencyProperty.Register(nameof(Colors), typeof(WpfObservableRangeCollection<SKColor>), typeof(PaletteColorAdder), new PropertyMetadata(default(WpfObservableRangeCollection<SKColor>), OnColorsChanged));

    private static void OnColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        PaletteColorAdder adder = (PaletteColorAdder)d;
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

    private void Colors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateAddSwatchesButton();
        UpdateAddButton();
    }

    private void UpdateAddButton()
    {
        AddButton.IsEnabled = !Colors.Contains(ToSKColor(SelectedColor)) && SelectedColor.A == 255;
    }

    private static void OnSwatchesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        PaletteColorAdder adder = (PaletteColorAdder)d;
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
        AddFromSwatches.IsEnabled = Swatches != null && Swatches.Where(x => x.Alpha == 255).Any(x => !Colors.Contains(x));
    }

    public PaletteColorAdder()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        SKColor color = ToSKColor(SelectedColor);
        if (!Colors.Contains(color))
        {
            Colors.Add(color.WithAlpha(255));
            AddButton.IsEnabled = false;
        }
    }

    private void PortableColorPicker_ColorChanged(object sender, RoutedEventArgs e) =>
        AddButton.IsEnabled = !Colors.Contains(ToSKColor(SelectedColor));

    private static SKColor ToSKColor(Color color) => new SKColor(color.R, color.G, color.B, color.A);

    private void AddFromSwatches_OnClick(object sender, RoutedEventArgs e)
    {
        if (Swatches == null) return;

        foreach (var color in Swatches)
        {
            if (color.Alpha < 255) continue; // No alpha support for now, palette colors shouldn't be transparent

            if (!Colors.Contains(color))
            {
                Colors.Add(color);
            }
        }

        AddFromSwatches.IsEnabled = false;
    }
}
