using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PixiEditor.ViewModels.SubViewModels.Main;
using System.Diagnostics;
using System.Linq;
using PixiEditor.Views.Dialogs;

namespace PixiEditor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        public new ViewModelMain DataContext { get => (ViewModelMain)base.DataContext; set => base.DataContext = value; }

        public MainWindow()
        {
            InitializeComponent();

            IServiceCollection services = new ServiceCollection()
                .AddSingleton<IPreferences>(new PreferencesSettings())
                .AddSingleton(new StylusViewModel());

            DataContext = new ViewModelMain(services.BuildServiceProvider());

            StateChanged += MainWindowStateChangeRaised;
            Activated += MainWindow_Activated;

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            DataContext.CloseAction = Close;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            DataContext.CloseWindow(e);
            DataContext.DiscordViewModel.Dispose();
        }

        [Conditional("RELEASE")]
        private static void CloseHelloThereIfRelease()
        {
            Application.Current.Windows.OfType<HelloTherePopup>().ToList().ForEach(x => x.Close());
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

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            CloseHelloThereIfRelease();
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
