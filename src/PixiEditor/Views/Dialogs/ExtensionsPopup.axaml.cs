using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PixiEditor.ViewModels.ExtensionManager;

namespace PixiEditor.Views.Dialogs;

public partial class ExtensionsPopup : PixiEditorPopup
{
    public ExtensionsPopup()
    {
        InitializeComponent();
    }

    // TODO: for testing
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is ExtensionManagerViewModel vm)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await vm.FetchAvailableExtensions());
            Dispatcher.UIThread.Invoke(() => vm.FetchOwnedExtensions());
        }
    }
}

