using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using PixiEditor.Extensions.UI;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Dialogs;

internal partial class DialogTitleBar : UserControl, ICustomTranslatorElement
{
    public static readonly StyledProperty<bool> CanMinimizeProperty = AvaloniaProperty.Register<DialogTitleBar, bool>(
        nameof(CanMinimize), defaultValue: true);

    public static readonly StyledProperty<bool> CanFullscreenProperty = AvaloniaProperty.Register<DialogTitleBar, bool>(
        nameof(CanFullscreen), defaultValue: true);

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        captionButtons.Attach(VisualRoot as Window);
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

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        if (CloseCommand is { } command)
        {
            if (command.CanExecute(null))
                command.Execute(null);
            return;
        }

        ((Window?)VisualRoot)?.Close();
    }

    private void MaximizeWindow(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window || !CanFullscreen)
            return;
        window.WindowState = WindowState.Maximized;
    }

    private void RestoreWindow(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window || !CanFullscreen)
            return;
        window.WindowState = WindowState.Normal;
    }

    private void MinimizeWindow(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window || !CanMinimize)
            return;
        window.WindowState = WindowState.Minimized;
    }
}
