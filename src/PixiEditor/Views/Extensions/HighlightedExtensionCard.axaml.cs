using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views.Extensions;

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

