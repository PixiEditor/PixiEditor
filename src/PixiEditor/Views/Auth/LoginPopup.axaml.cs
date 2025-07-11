using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Views.Auth;

public partial class LoginPopup : PixiEditorPopup
{
    public LoginPopup()
    {
        InitializeComponent();
        DataContext = ViewModelMain.Current.UserViewModel;
    }

    protected override async void OnGotFocus(GotFocusEventArgs e)
    {
        if (DataContext is UserViewModel { WaitingForActivation: true } vm)
        {
            await vm.TryValidateSession();
        }
    }
}

