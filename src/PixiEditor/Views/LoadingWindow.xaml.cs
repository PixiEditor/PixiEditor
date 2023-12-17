using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PixiEditor.Views;

public partial class LoadingWindow : Window
{
    public static LoadingWindow Instance { get; private set; }

    public ImageSource LoadingImage { get; } = new BitmapImage(new Uri("pack://application:,,,/images/processing.gif"));
    
    public LoadingWindow()
    {
        InitializeComponent();
    }

    public static void ShowInNewThread()
    {
        var thread = new Thread(ThreadStart) { IsBackground = true };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    public void SafeClose()
    {
        Dispatcher.Invoke(Close);
    }

    private static void ThreadStart()
    {
        Instance = new LoadingWindow();
        Instance.Show();

        Instance.Closed += (_, _) =>
            Instance.Dispatcher.InvokeShutdown();

        Dispatcher.Run();
    }
}

