using System.Windows;
using System.Windows.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Helpers;
using PixiEditor.Models.Localization;

namespace PixiEditor.Views.Dialogs;

public partial class AboutPopup : Window
{
    public static LocalizedString VersionText =>
        new LocalizedString("VERSION", VersionHelpers.GetCurrentAssemblyVersionString(true));

    public bool DisplayDonationButton
    {
#if STEAM
        get => false;
#else
        get => true;
#endif
    }
    public AboutPopup()
    {
        InitializeComponent();
    }
    
    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }
}

