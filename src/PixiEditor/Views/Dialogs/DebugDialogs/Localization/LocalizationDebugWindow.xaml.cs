using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.Dialogs.DebugDialogs.Localization;

public partial class LocalizationDebugWindow : Window
{
    private static LocalizationDataContext dataContext;
    private bool passedStartup;

    public LocalizationDebugWindow()
    {
        InitializeComponent();
        DataContext = (dataContext ??= new LocalizationDataContext());
    }

    private void ApiKeyChanged(object sender, TextChangedEventArgs e)
    {
        if (!passedStartup)
        {
            passedStartup = true;
            return;
        }
        
        dataContext.LoggedIn = false;
        dataContext.StatusMessage = "NOT_LOGGED_IN";
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }
}
