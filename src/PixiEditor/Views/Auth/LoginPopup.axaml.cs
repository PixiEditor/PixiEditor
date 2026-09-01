using Avalonia.Input;
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

    protected override async void OnGotFocus(FocusChangedEventArgs e)
    {
        if (DataContext is UserViewModel { WaitingForActivation: true } vm)
        {
            await vm.TryValidateSession();
        }
    }
}

