using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views.Extensions;

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

