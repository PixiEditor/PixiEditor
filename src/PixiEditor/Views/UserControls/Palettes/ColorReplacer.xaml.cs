using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BackendColor = PixiEditor.DrawingApi.Core.ColorsImpl.Color;
using BackendColors = PixiEditor.DrawingApi.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.UserControls.Palettes;

internal partial class ColorReplacer : UserControl
{
    public BackendColor ColorToReplace
    {
        get { return (BackendColor)GetValue(ColorToReplaceProperty); }
        set { SetValue(ColorToReplaceProperty, value); }
    }


    public static readonly DependencyProperty ColorToReplaceProperty =
        DependencyProperty.Register(nameof(ColorToReplace), typeof(BackendColor), typeof(ColorReplacer), new PropertyMetadata(BackendColors.White));


    public Color HintColor
    {
        get { return (Color)GetValue(HintColorProperty); }
        set { SetValue(HintColorProperty, value); }
    }

    public Color NewColor
    {
        get { return (Color)GetValue(NewColorProperty); }
        set { SetValue(NewColorProperty, value); }
    }

    public static readonly DependencyProperty ReplaceColorsCommandProperty = DependencyProperty.Register(nameof(ReplaceColorsCommand), typeof(ICommand), typeof(ColorReplacer), new PropertyMetadata(default(ICommand)));

    public ICommand ReplaceColorsCommand
    {
        get { return (ICommand)GetValue(ReplaceColorsCommandProperty); }
        set { SetValue(ReplaceColorsCommandProperty, value); }
    }



    public static readonly DependencyProperty NewColorProperty =
        DependencyProperty.Register(nameof(NewColor), typeof(Color), typeof(ColorReplacer), new PropertyMetadata(Colors.Black));



    public static readonly DependencyProperty HintColorProperty =
        DependencyProperty.Register(nameof(HintColor), typeof(Color), typeof(ColorReplacer), new PropertyMetadata(Colors.Black));



    public bool IsCollapsed
    {
        get { return (bool)GetValue(IsCollapsedProperty); }
        set { SetValue(IsCollapsedProperty, value); }
    }


    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(ColorReplacer), new PropertyMetadata(false));



    private void UIElement_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(PaletteColor.PaletteColorDaoFormat))
        {
            string hex = (string)e.Data.GetData(PaletteColor.PaletteColorDaoFormat);
            ColorToReplace = BackendColor.Parse(hex).WithAlpha(255);
        }
    }

    public ColorReplacer()
    {
        InitializeComponent();
    }

    private void ReplaceButton_OnClick(object sender, RoutedEventArgs e)
    {
        BackendColor first = ColorToReplace.WithAlpha(255);
        Color rawSecond = NewColor;

        BackendColor second = new BackendColor(rawSecond.R, rawSecond.G, rawSecond.B, 255);

        var pack = (first, second);
        if (ReplaceColorsCommand.CanExecute(pack))
        {
            ReplaceColorsCommand.Execute(pack);
        }

        ColorToReplace = BackendColors.White;
    }
}
