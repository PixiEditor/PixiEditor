using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.ViewModels.UserPreferences;

namespace PixiEditor.Views.Dialogs;

public partial class OnboardingDialog : Window
{
    public OnboardingDialog()
    {
        InitializeComponent();
    }

    public void Finish()
    {
        if (DataContext is OnboardingViewModel vm)
        {
            vm.OnFinish();
        }

        Close();
    }
}

