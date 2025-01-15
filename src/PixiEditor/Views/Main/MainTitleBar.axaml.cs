using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PixiEditor.ViewModels.Menu;

namespace PixiEditor.Views.Main;

public partial class MainTitleBar : UserControl {
    
    public MainTitleBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is MenuBarViewModel menuBarViewModel)
        {
            NativeMenu.SetMenu(MainWindow.Current, menuBarViewModel.NativeMenu);
        }
    }
}

