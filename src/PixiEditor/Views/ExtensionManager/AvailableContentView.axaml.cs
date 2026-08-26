using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views.ExtensionManager;

public partial class AvailableContentView : UserControl
{
    public static readonly StyledProperty<ICommand> SelectExtensionCommandProperty = AvaloniaProperty.Register<AvailableContentView, ICommand>(
        nameof(SelectExtensionCommand));

    public ICommand SelectExtensionCommand
    {
        get => GetValue(SelectExtensionCommandProperty);
        set => SetValue(SelectExtensionCommandProperty, value);
    }

    public AvailableContentView()
    {
        InitializeComponent();
    }
}

