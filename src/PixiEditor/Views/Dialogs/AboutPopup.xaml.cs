using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views.Dialogs;

public partial class AboutPopup : Window
{
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

