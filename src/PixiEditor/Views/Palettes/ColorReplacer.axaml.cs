using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.Views.Palettes;

internal partial class ColorReplacer : UserControl
{
    public static readonly StyledProperty<PaletteColor> ColorToReplaceProperty =
        AvaloniaProperty.Register<ColorReplacer, PaletteColor>(
            nameof(ColorToReplace),
            PaletteColor.White);

    public PaletteColor ColorToReplace
    {
        get => GetValue(ColorToReplaceProperty);
        set => SetValue(ColorToReplaceProperty, value);
    }

    public static readonly StyledProperty<Color> HintColorProperty =
        AvaloniaProperty.Register<ColorReplacer, Color>(
            nameof(HintColor),
            Colors.Black);

    public Color HintColor
    {
        get => GetValue(HintColorProperty);
        set => SetValue(HintColorProperty, value);
    }

    public static readonly StyledProperty<Color> NewColorProperty =
        AvaloniaProperty.Register<ColorReplacer, Color>(
            nameof(NewColor),
            Colors.Black);

    public Color NewColor
    {
        get => GetValue(NewColorProperty);
        set => SetValue(NewColorProperty, value);
    }

    public static readonly StyledProperty<ICommand> ReplaceColorsCommandProperty =
        AvaloniaProperty.Register<ColorReplacer, ICommand>(
            nameof(ReplaceColorsCommand),
            default(ICommand));

    public ICommand ReplaceColorsCommand
    {
        get => GetValue(ReplaceColorsCommandProperty);
        set => SetValue(ReplaceColorsCommandProperty, value);
    }

    public static readonly StyledProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.Register<ColorReplacer, bool>(
            nameof(IsCollapsed),
            false);

    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public ColorReplacer()
    {
        InitializeComponent();
        DropTarget.AddHandler(DragDrop.DropEvent, PaletteColorControl_OnDrop);
    }

    private void PaletteColorControl_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(PaletteColorControl.PaletteColorDaoFormat))
        {
            string hex = (string)e.Data.Get(PaletteColorControl.PaletteColorDaoFormat);
            if (hex is null)
            {
                return;
            }

            ColorToReplace = PaletteColor.Parse(hex);
        }
    }

    private void ReplaceButton_OnClick(object sender, RoutedEventArgs e)
    {
        PaletteColor first = ColorToReplace;
        Color rawSecond = NewColor;

        PaletteColor second = new PaletteColor(rawSecond.R, rawSecond.G, rawSecond.B);

        var pack = (first, second);
        if (ReplaceColorsCommand.CanExecute(pack))
        {
            ReplaceColorsCommand.Execute(pack);
        }

        ColorToReplace = second;
        NewColor = new Color(255, first.R, first.G, first.B);
    }
}
