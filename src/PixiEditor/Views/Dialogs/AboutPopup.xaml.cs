using System.Windows;
using System.Windows.Input;
using PixiEditor.Helpers;

namespace PixiEditor.Views.Dialogs;

public partial class AboutPopup : Window
{
    public static string VersionText => $"Version: {VersionHelpers.GetCurrentAssemblyVersionString()}";
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

