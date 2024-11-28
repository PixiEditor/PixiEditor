using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PixiEditor.UpdateInstaller.New.ViewModels;

namespace PixiEditor.UpdateInstaller.New.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        MainViewModel vmm = (MainViewModel)DataContext;
        await Task.Run(() =>
        {
            try
            {
                vmm.InstallUpdate();
            }
            catch (Exception ex)
            {
                File.AppendAllText("ErrorLog.txt", $"Error PixiEditor.UpdateInstaller: {DateTime.Now}\n{ex.Message}\n{ex.StackTrace}\n-----\n");

                Dispatcher.UIThread.Invoke(() =>
                {
                    var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow("Update error", ex.Message);
                    messageBoxStandardWindow.Show();
                });
            }
            finally
            {
                var files = Directory.GetFiles(vmm.UpdateDirectory, "PixiEditor.exe");
                if (files.Length > 0)
                {
                    string pixiEditorExecutablePath = files[0];
                    Process.Start(pixiEditorExecutablePath);
                }
            }
        });

        this.Close();
    }
}
