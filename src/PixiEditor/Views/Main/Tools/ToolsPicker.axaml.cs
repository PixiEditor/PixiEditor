using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Views.Main.Tools;

internal partial class ToolsPicker : UserControl
{
    public static readonly StyledProperty<IToolSetHandler> ToolSetProperty =
        AvaloniaProperty.Register<ToolsPicker, IToolSetHandler>(
            nameof(ToolSet));
    public IToolSetHandler ToolSet
    {
        get => GetValue(ToolSetProperty);
        set => SetValue(ToolSetProperty, value);
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

