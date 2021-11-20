using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels;
using PixiEditor.Views.Dialogs;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PixiEditor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private static WriteableBitmap pixiEditorLogo;

        private readonly IPreferences preferences;

        public static MainWindow Current { get; private set; }

        public new ViewModelMain DataContext { get => (ViewModelMain)base.DataContext; set => base.DataContext = value; }

        public MainWindow()
        {
            IServiceProvider services = new ServiceCollection()
                .AddPixiEditor()
                .BuildServiceProvider();

            preferences = services.GetRequiredService<IPreferences>();
            DataContext = services.GetRequiredService<ViewModelMain>();
            DataContext.Setup(services);

            InitializeComponent();

            pixiEditorLogo = BitmapFactory.FromResource(@"/Images/PixiEditorLogo.png");

            UpdateWindowChromeBorderThickness();
            StateChanged += MainWindow_StateChanged;
            Activated += MainWindow_Activated;

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

            OnReleaseBuild();
        }

        public static MainWindow CreateWithDocuments(IEnumerable<Document> documents)
        {
            MainWindow window = new();

            BitmapManager bitmapManager = window.DataContext.BitmapManager;
            bitmapManager.Documents.AddRange(documents);
            bitmapManager.ActiveDocument = bitmapManager.Documents.FirstOrDefault();

            return window;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            DataContext.CloseWindow(e);
            DataContext.DiscordViewModel.Dispose();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(Helpers.WindowSizeHelper.SetMaxSizeHook);
        }

        [Conditional("RELEASE")]
        private static void CloseHelloThereIfRelease()
        {
            Application.Current.Windows.OfType<HelloTherePopup>().ToList().ForEach(x => { if (!x.IsClosing) x.Close(); });
        }

        [Conditional("RELEASE")]
        private void OnReleaseBuild()
        {
            rawLayerAnchorable.Hide();
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

        private void UpdateWindowChromeBorderThickness()
        {
            if (WindowState == WindowState.Maximized)
            {
                windowsChrome.ResizeBorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                windowsChrome.ResizeBorderThickness = new Thickness(5, 5, 5, 5);
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            UpdateWindowChromeBorderThickness();

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
