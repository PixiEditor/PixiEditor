using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PixiEditor.OperatingSystem;
using PixiEditor.ViewModels.Menu;

namespace PixiEditor.Views.Main;

public partial class MainTitleBar : UserControl
{

    private MiniAnimationPlayer miniPlayer;
    public MainTitleBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        miniPlayer = this.FindControl<MiniAnimationPlayer>("MiniPlayer");
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (miniPlayer != null)
        {
            miniPlayer.IsVisible = e.NewSize.Width > 1165;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (IOperatingSystem.Current.IsMacOs && DataContext is MenuBarViewModel menuBarViewModel)
        {
            NativeMenu.SetMenu(MainWindow.Current, menuBarViewModel.NativeMenu);
        }
    }
}

