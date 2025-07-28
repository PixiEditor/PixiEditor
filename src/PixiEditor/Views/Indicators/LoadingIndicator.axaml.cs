using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views.Indicators;

public partial class LoadingIndicator : UserControl
{
    public LoadingIndicator()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

