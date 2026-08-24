using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using PixiEditor.Extensions.UI;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Dialogs;

internal partial class DialogTitleBar : UserControl, ICustomTranslatorElement
{
    public static readonly StyledProperty<bool> CanMinimizeProperty = AvaloniaProperty.Register<DialogTitleBar, bool>(
        nameof(CanMinimize), defaultValue: true);

    public static readonly StyledProperty<bool> CanFullscreenProperty = AvaloniaProperty.Register<DialogTitleBar, bool>(
        nameof(CanFullscreen), defaultValue: true);

    public static readonly StyledProperty<bool> HideIfSystemDecorationsProperty =
        AvaloniaProperty.Register<DialogTitleBar, bool>(
            nameof(HideIfSystemDecorations));

    public bool HideIfSystemDecorations
    {
        get => GetValue(HideIfSystemDecorationsProperty);
        set => SetValue(HideIfSystemDecorationsProperty, value);
    }

    public static readonly StyledProperty<string> TitleKeyProperty =
        AvaloniaProperty.Register<DialogTitleBar, string>(nameof(TitleKey), string.Empty);

    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<DialogTitleBar, ICommand?>(nameof(CloseCommand));

    public static readonly StyledProperty<Control> AdditionalElementProperty =
        AvaloniaProperty.Register<DialogTitleBar, Control>("AdditionalElement");

    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    /// <summary>
    /// The localization key of the window's title
    /// </summary>
    public string TitleKey
    {
        get => GetValue(TitleKeyProperty);
        set => SetValue(TitleKeyProperty, value);
    }

    public bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }

    public bool CanFullscreen
    {
        get => GetValue(CanFullscreenProperty);
        set => SetValue(CanFullscreenProperty, value);
    }

    public Control AdditionalElement
    {
        get { return (Control)GetValue(AdditionalElementProperty); }
        set { SetValue(AdditionalElementProperty, value); }
    }

    public DialogTitleBar()
    {
        InitializeComponent();
    }

    void ICustomTranslatorElement.SetTranslationBinding(AvaloniaProperty dependencyProperty,
        IObservable<string> binding)
    {
        Bind(dependencyProperty, binding);
    }

    AvaloniaProperty ICustomTranslatorElement.GetDependencyProperty()
    {
        return TitleKeyProperty;
    }
}
