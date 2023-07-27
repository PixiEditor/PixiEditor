using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views.Dialogs;

internal partial class DialogTitleBar : UserControl, ICustomTranslatorElement
{
    public static readonly StyledProperty<string> TitleKeyProperty =
        AvaloniaProperty.Register<DialogTitleBar, string>(nameof(TitleKey), string.Empty);

    public static readonly StyledProperty<ICommand> CloseCommandProperty =
        AvaloniaProperty.Register<DialogTitleBar, ICommand>(nameof(CloseCommand));

    public ICommand CloseCommand
    {
        get { return (ICommand)GetValue(CloseCommandProperty); }
        set { SetValue(CloseCommandProperty, value); }
    }

    /// <summary>
    /// The localization key of the window's title
    /// </summary>
    public string TitleKey
    {
        get { return (string)GetValue(TitleKeyProperty); }
        set { SetValue(TitleKeyProperty, value); }
    }

    public DialogTitleBar()
    {
        InitializeComponent();
    }

    void ICustomTranslatorElement.SetTranslationBinding(AvaloniaProperty dependencyProperty, IObservable<string> binding)
    {
        Bind(dependencyProperty, binding);
    }

    AvaloniaProperty ICustomTranslatorElement.GetDependencyProperty()
    {
        return TitleKeyProperty;
    }
}
