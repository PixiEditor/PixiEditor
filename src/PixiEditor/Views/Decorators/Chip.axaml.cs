using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.Extensions.UI;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Decorators;

internal partial class Chip : UserControl, ICustomTranslatorElement
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<Chip, string>(nameof(Text));

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public static readonly StyledProperty<SolidColorBrush> OutlineColorProperty =
        AvaloniaProperty.Register<Chip, SolidColorBrush>(nameof(OutlineColor));

    public SolidColorBrush OutlineColor
    {
        get { return (SolidColorBrush)GetValue(OutlineColorProperty); }
        set { SetValue(OutlineColorProperty, value); }
    }
    public Chip()
    {
        InitializeComponent();
    }

    public void SetTranslationBinding(AvaloniaProperty dependencyProperty, IObservable<string> binding)
    {
        Bind(dependencyProperty, binding);
    }

    public AvaloniaProperty GetDependencyProperty()
    {
        return TextProperty;
    }
}

