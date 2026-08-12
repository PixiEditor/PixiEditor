using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views.Extensions;

public partial class AvailableExtensionCard : UserControl
{
    public static readonly StyledProperty<ICommand> SelectExtensionCommandProperty = AvaloniaProperty.Register<AvailableExtensionCard, ICommand>(
        nameof(SelectExtensionCommand));

    public ICommand SelectExtensionCommand
    {
        get => GetValue(SelectExtensionCommandProperty);
        set => SetValue(SelectExtensionCommandProperty, value);
    }

    public AvailableExtensionCard()
    {
        InitializeComponent();
    }
}

