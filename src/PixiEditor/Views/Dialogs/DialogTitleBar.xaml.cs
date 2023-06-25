using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PixiEditor.Views.Dialogs;

internal partial class DialogTitleBar : UserControl, ICustomTranslatorElement
{
    public static readonly DependencyProperty TitleKeyProperty =
        DependencyProperty.Register(nameof(TitleKey), typeof(string), typeof(DialogTitleBar), new PropertyMetadata(""));

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(DialogTitleBar), new PropertyMetadata(null));

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

    void ICustomTranslatorElement.SetTranslationBinding(DependencyProperty dependencyProperty, Binding binding)
    {
        SetBinding(dependencyProperty, binding);
    }

    DependencyProperty ICustomTranslatorElement.GetDependencyProperty()
    {
        return TitleKeyProperty;
    }
}
