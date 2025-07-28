using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.Views.Palettes;

internal partial class SwatchesView : UserControl
{
    public static readonly StyledProperty<ObservableCollection<PaletteColor>> SwatchesProperty =
        AvaloniaProperty.Register<SwatchesView, ObservableCollection<PaletteColor>>(
            nameof(Swatches));

    public ObservableCollection<PaletteColor> Swatches
    {
        get => GetValue(SwatchesProperty);
        set => SetValue(SwatchesProperty, value);
    }

    public static readonly StyledProperty<ICommand> MouseDownCommandProperty = AvaloniaProperty.Register<SwatchesView, ICommand>(
        nameof(MouseDownCommand));

    public ICommand MouseDownCommand
    {
        get => GetValue(MouseDownCommandProperty);
        set => SetValue(MouseDownCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> SelectSwatchCommandProperty = AvaloniaProperty.Register<SwatchesView, ICommand>(
        "SelectSwatchCommand");

    public ICommand SelectSwatchCommand
    {
        get => GetValue(SelectSwatchCommandProperty);
        set => SetValue(SelectSwatchCommandProperty, value);
    }

    public SwatchesView()
    {
        InitializeComponent();
    }
}

