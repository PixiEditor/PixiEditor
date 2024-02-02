using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.Views.Main;

internal partial class ToolsPicker : UserControl
{
    public static readonly StyledProperty<ObservableCollection<IToolHandler>> ToolsProperty =
        AvaloniaProperty.Register<ToolsPicker, ObservableCollection<IToolHandler>>(nameof(Tools));

    public ObservableCollection<IToolHandler> Tools
    {
        get => GetValue(ToolsProperty);
        set => SetValue(ToolsProperty, value);
    }

    public ToolsPicker()
    {
        InitializeComponent();
    }
}

