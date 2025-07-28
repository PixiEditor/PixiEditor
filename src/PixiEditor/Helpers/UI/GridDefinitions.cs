using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Helpers.UI;

public class GridDefinitions : AvaloniaObject
{
    public static readonly AttachedProperty<ColumnDefinitions> ColumnDefinitionsBindableProperty =
        AvaloniaProperty.RegisterAttached<GridDefinitions, Grid, ColumnDefinitions>("ColumnDefinitionsBindable");

    public static void SetColumnDefinitionsBindable(Grid obj, ColumnDefinitions value)
    {
        obj.SetValue(ColumnDefinitionsBindableProperty, value);
        obj.ColumnDefinitions = value;
    }

    public static ColumnDefinitions GetColumnDefinitionsBindable(Grid obj) => obj.GetValue(ColumnDefinitionsBindableProperty);
    
    public static readonly AttachedProperty<RowDefinitions> RowDefinitionsBindableProperty =
        AvaloniaProperty.RegisterAttached<GridDefinitions, Grid, RowDefinitions>("RowDefinitionsBindable");
    
    public static void SetRowDefinitionsBindable(Grid obj, RowDefinitions value)
    {
        obj.SetValue(RowDefinitionsBindableProperty, value);
        obj.RowDefinitions = value;
    }

    public static RowDefinitions GetRowDefinitionsBindable(Grid obj) => obj.GetValue(RowDefinitionsBindableProperty);
}
