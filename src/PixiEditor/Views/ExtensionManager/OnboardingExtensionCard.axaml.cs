using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views.ExtensionManager;

public partial class OnboardingExtensionCard : UserControl
{
    public static readonly StyledProperty<ICommand> SelectExtensionCommandProperty = AvaloniaProperty.Register<OnboardingExtensionCard, ICommand>(
        nameof(SelectExtensionCommand));

    public ICommand SelectExtensionCommand
    {
        get => GetValue(SelectExtensionCommandProperty);
        set => SetValue(SelectExtensionCommandProperty, value);
    }

    public OnboardingExtensionCard()
    {
        InitializeComponent();
    }
}

