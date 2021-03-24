using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Processes;
using PixiEditor.Models.UserPreferences;
using PixiEditor.UpdateModule;
using PixiEditor.ViewModels;

namespace PixiEditor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModelMain viewModel;

        public MainWindow()
        {
            InitializeComponent();

            IServiceCollection services = new ServiceCollection()
                .AddSingleton<IPreferences>(new PreferencesSettings());

            DataContext = new ViewModelMain(services.BuildServiceProvider());

            StateChanged += MainWindowStateChangeRaised;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            viewModel = (ViewModelMain)DataContext;
            viewModel.CloseAction = Close;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ((ViewModelMain)DataContext).CloseWindow(e);
            viewModel.DiscordViewModel.Dispose();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Helpers.CrashHelper.SaveCrashInfo((Exception)e.ExceptionObject);
        }
    }
}