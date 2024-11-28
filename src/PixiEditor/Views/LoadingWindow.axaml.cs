using System.Threading;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PixiEditor.Views;

public partial class LoadingWindow : Window
{
    public static LoadingWindow Instance { get; private set; }
    
    public LoadingWindow()
    {
        InitializeComponent();
    }

    public static void ShowInNewThread()
    {
        /*var thread = new Thread(ThreadStart) { IsBackground = true };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();*/
    }

    public void SafeClose()
    {
        Dispatcher.UIThread.Invoke(Close);
    }

    private static void ThreadStart()
    {
        Dispatcher.UIThread.Invoke(ThreadStartInternal);
    }

    private static void ThreadStartInternal()
    {
        Instance = new LoadingWindow();
        Instance.Show();
    }
}

