using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views.ExtensionManager;

public partial class HighlightedExtensionCard : UserControl
{
    public static readonly StyledProperty<ICommand> SelectCommandProperty = AvaloniaProperty.Register<HighlightedExtensionCard, ICommand>(
        nameof(SelectCommand));

    public ICommand SelectCommand
    {
        get => GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
    }

    public HighlightedExtensionCard()
    {
        InitializeComponent();
    }
}

