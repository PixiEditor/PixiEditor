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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if(DataContext is OnboardingViewModel vm)
        {
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(OnboardingViewModel.Page))
                {
                    if (vm.Page == 2)
                    {
                        finishAnimation.Start();
                    }
                }
            };
        }
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

