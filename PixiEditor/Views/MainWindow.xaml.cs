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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;

namespace PixiEditor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private static WriteableBitmap pixiEditorLogo;

        private PreferencesSettings preferences;

        public new ViewModelMain DataContext { get => (ViewModelMain)base.DataContext; set => base.DataContext = value; }

        public MainWindow()
        {
            preferences = new PreferencesSettings();

            IServiceCollection services = new ServiceCollection()
                .AddSingleton<IPreferences>(preferences)
                .AddSingleton<StylusViewModel>()
                .AddSingleton<WindowViewModel>();

            DataContext = new ViewModelMain(services.BuildServiceProvider());

            InitializeComponent();

            pixiEditorLogo = BitmapFactory.FromResource(@"/Images/PixiEditorLogo.png");

            StateChanged += MainWindowStateChangeRaised;
            Activated += MainWindow_Activated;

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            rawLayerAnchorable.IsVisible = viewModel.IsDebug;
            DataContext.CloseAction = Close;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            DataContext.BitmapManager.DocumentChanged += BitmapManager_DocumentChanged;
            preferences.AddCallback<bool>("ImagePreviewInTaskbar", x =>
            {
                if (x)
                {
                    UpdateTaskbarIcon(DataContext.BitmapManager.ActiveDocument);
                }
                else
                {
                    UpdateTaskbarIcon(null);
                }
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            DataContext.CloseWindow(e);
            DataContext.DiscordViewModel.Dispose();
        }

        [Conditional("RELEASE")]
        private static void CloseHelloThereIfRelease()
        {
            Application.Current.Windows.OfType<HelloTherePopup>().ToList().ForEach(x => { if (!x.IsClosing) x.Close(); });
        }

        private void BitmapManager_DocumentChanged(object sender, Models.Events.DocumentChangedEventArgs e)
        {
            if (preferences.GetPreference("ImagePreviewInTaskbar", false))
            {
                UpdateTaskbarIcon(e.NewDocument);
            }
        }

        private void UpdateTaskbarIcon(Document document)
        {
            if (document?.PreviewImage == null)
            {
                Icon = pixiEditorLogo;
                return;
            }

            var previewCopy = document.PreviewImage.Clone()
                .Resize(512, 512, WriteableBitmapExtensions.Interpolation.NearestNeighbor);

            previewCopy.Blit(new Rect(256, 256, 256, 256), pixiEditorLogo, new Rect(0, 0, 512, 512));

            Icon = previewCopy;
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

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                DataContext.FileSubViewModel.Open(files[0]);
            }
        }
    }
}
