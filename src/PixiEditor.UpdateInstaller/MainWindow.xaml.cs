using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PixiEditor.UpdateInstaller;

/// <summary>
///     Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ViewModelMain();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModelMain vmm = (ViewModelMain)DataContext;
        await Task.Run(() =>
        {
            try
            {
                vmm.InstallUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update error", MessageBoxButton.OK, MessageBoxImage.Error);
                File.AppendAllText("ErrorLog.txt", $"Error PixiEditor.UpdateInstaller: {DateTime.Now}\n{ex.Message}\n{ex.StackTrace}\n-----\n");
            }
            finally
            {
                string pixiEditorExecutablePath = Directory.GetFiles(vmm.UpdateDirectory, "PixiEditor.exe")[0];
                Process.Start(pixiEditorExecutablePath);
            }
        });
        Close();
    }
}