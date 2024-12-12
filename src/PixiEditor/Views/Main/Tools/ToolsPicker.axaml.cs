using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Views.Main.Tools;

internal partial class ToolsPicker : UserControl
{
    public static readonly StyledProperty<ObservableCollection<IToolSetHandler>> ToolSetsProperty = AvaloniaProperty.Register<ToolsPicker, ObservableCollection<IToolSetHandler>>("ToolSets");

    public static readonly StyledProperty<IToolSetHandler> ToolSetProperty =
        AvaloniaProperty.Register<ToolsPicker, IToolSetHandler>(
            nameof(ToolSet));
    public IToolSetHandler ToolSet
    {
        get => GetValue(ToolSetProperty);
        set => SetValue(ToolSetProperty, value);
    }

    public ObservableCollection<IToolSetHandler> ToolSets
    {
        get { return (ObservableCollection<IToolSetHandler>)GetValue(ToolSetsProperty); }
        set { SetValue(ToolSetsProperty, value); }
    }
    
    public static readonly StyledProperty<ICommand> SwitchToolSetCommandProperty = AvaloniaProperty.Register<ToolsPicker, ICommand>(
        "SwitchToolSetCommand");
    
    public ICommand SwitchToolSetCommand
    {
        get => GetValue(SwitchToolSetCommandProperty);
        set => SetValue(SwitchToolSetCommandProperty, value);
    }
    
    public ToolsPicker()
    {
        InitializeComponent();
    }
}

