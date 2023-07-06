using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace PixiEditor.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        autoComplete.ItemsSource = new List<string> { "Wendy's", "Boob guy", "Patrick" };
    }

}
