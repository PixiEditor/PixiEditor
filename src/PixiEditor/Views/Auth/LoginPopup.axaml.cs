using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Views.Auth;

public partial class LoginPopup : PixiEditorPopup
{
    public LoginPopup()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is UserViewModel vm)
        {
            vm.PropertyChanged += VmOnPropertyChanged;
            Height = vm.IsLoggedIn ? 245 : 190;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (DataContext is UserViewModel vm)
        {
            vm.PropertyChanged -= VmOnPropertyChanged;
        }
    }

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UserViewModel.IsLoggedIn))
        {
            Height = (DataContext as UserViewModel)?.IsLoggedIn == true ? 245 : 190;
        }
    }

    protected override async void OnGotFocus(GotFocusEventArgs e)
    {
        if (DataContext is UserViewModel { WaitingForActivation: true } vm)
        {
            await vm.TryValidateSession();
        }
    }
}

