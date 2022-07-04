using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.Dialogs;

public partial class DialogTitleBar : UserControl
{
    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(DialogTitleBar), new PropertyMetadata(""));

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(DialogTitleBar), new PropertyMetadata(null));

    public ICommand CloseCommand
    {
        get { return (ICommand)GetValue(CloseCommandProperty); }
        set { SetValue(CloseCommandProperty, value); }
    }

    public string TitleText
    {
        get { return (string)GetValue(TitleTextProperty); }
        set { SetValue(TitleTextProperty, value); }
    }

    public DialogTitleBar()
    {
        InitializeComponent();
    }
}