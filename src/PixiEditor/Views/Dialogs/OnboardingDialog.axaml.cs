using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.Common.Performance;
using PixiEditor.ViewModels.UserPreferences;

namespace PixiEditor.Views.Dialogs;

public partial class OnboardingDialog : Window
{
    public OnboardingDialog()
    {
        using PerfMeasure _ = new PerfMeasure(PerfEventType.OnboardingDialog_Constructor);
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

