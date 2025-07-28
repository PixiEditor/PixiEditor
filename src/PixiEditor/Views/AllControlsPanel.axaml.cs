using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views;

public partial class AllControlsPanel : UserControl
{
    public AllControlsPanel()
    {
        InitializeComponent();
        autoComplete.ItemsSource = new List<string> { "Wendy's", "Boob guy", "Patrick" };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

