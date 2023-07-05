using System.Collections.Generic;
using Avalonia.Controls;

namespace PixiEditor.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        autoComplete.ItemsSource = new List<string> { "Wendy's", "Boob guy", "Patrick" };
    }
}
