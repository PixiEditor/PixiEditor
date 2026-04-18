using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is ExtensionManagerViewModel vm)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await vm.FetchAvailableExtensions());
            vm.FetchOwnedExtensions();
        }
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        if (DataContext is ExtensionManagerViewModel vm)
        {
            if (vm.ShouldUpdateUserOwnedProducts)
            {
                Dispatcher.UIThread.InvokeAsync(async () => await vm.UpdateUserOwnedProducts());
            }
        }
    }
}

