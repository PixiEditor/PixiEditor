using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Views.Input;

public partial class FontItemView : UserControl
{
    public static readonly StyledProperty<FontItem> ItemProperty = AvaloniaProperty.Register<FontItemView, FontItem>(
        nameof(Item));

    public FontItem Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public FontItemView()
    {
        InitializeComponent();
    }

    private void Favourite_OnClick(object? sender, RoutedEventArgs e)
    {
        Item.IsFavourite = !Item.IsFavourite;
        e.Handled = true;
    }
}

