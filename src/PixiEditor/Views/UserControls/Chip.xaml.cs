using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixiEditor.Platform;
using Brush = System.Drawing.Brush;

namespace PixiEditor.Views.UserControls;

internal partial class Chip : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(Chip), new PropertyMetadata(default(string)));

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public static readonly DependencyProperty OutlineColorProperty = DependencyProperty.Register(
        nameof(OutlineColor), typeof(SolidColorBrush), typeof(Chip), new PropertyMetadata(default(SolidColorBrush)));

    public SolidColorBrush OutlineColor
    {
        get { return (SolidColorBrush)GetValue(OutlineColorProperty); }
        set { SetValue(OutlineColorProperty, value); }
    }
    public Chip()
    {
        InitializeComponent();
    }
}

