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
    private Panel logoPanel;
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
        logoPanel = this.FindControl<Panel>("LogoPanel");

        if (IOperatingSystem.Current.IsMacOs && VisualRoot is Window window && logoPanel != null)
        {
            window.PropertyChanged += OnPropertyChanged;
        }
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(Window.WindowState) && logoPanel != null)
        {
            logoPanel.Margin = e.NewValue is WindowState and WindowState.FullScreen
                ? new Thickness(10, 0, 0, 0)
                : new Thickness(75, 0, 0, 0);
        }
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
            if (menuBarViewModel.NativeMenu != null)
            {
                NativeMenu.SetMenu(MainWindow.Current, menuBarViewModel.NativeMenu);
            }
            else
            {
                menuBarViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(MenuBarViewModel.NativeMenu))
                    {
                        if (menuBarViewModel.NativeMenu != null)
                        {
                            NativeMenu.SetMenu(MainWindow.Current, menuBarViewModel.NativeMenu);
                        }
                        else
                        {
                            NativeMenu.SetMenu(MainWindow.Current, null);
                        }
                    }
                };
            }
        }
    }
}

