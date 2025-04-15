using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views.Auth;

public partial class LoginForm : UserControl
{
    public static readonly StyledProperty<ICommand> RequestLoginCommandProperty = AvaloniaProperty.Register<LoginForm, ICommand>(
        nameof(RequestLoginCommand));

    public ICommand RequestLoginCommand
    {
        get => GetValue(RequestLoginCommandProperty);
        set => SetValue(RequestLoginCommandProperty, value);
    }

    public LoginForm()
    {
        InitializeComponent();
    }
}

