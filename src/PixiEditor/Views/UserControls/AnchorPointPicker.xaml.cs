using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.Views.UserControls;

/// <summary>
///     Interaction logic for AnchorPointPicker.xaml
/// </summary>
internal partial class AnchorPointPicker : UserControl
{
    public static readonly DependencyProperty AnchorPointProperty =
        DependencyProperty.Register(nameof(AnchorPoint), typeof(ResizeAnchor), typeof(AnchorPointPicker),
            new PropertyMetadata());


    private ToggleButton _selectedToggleButton;

    public AnchorPointPicker()
    {
        InitializeComponent();
    }

    public ResizeAnchor AnchorPoint
    {
        get => (ResizeAnchor)GetValue(AnchorPointProperty);
        set => SetValue(AnchorPointProperty, value);
    }

    private void ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        ToggleButton btn = (ToggleButton)sender;
        int row = Grid.GetRow(btn);
        int column = Grid.GetColumn(btn);
        AnchorPoint = (column, row) switch
        {
            (0, 0) => ResizeAnchor.TopLeft,
            (1, 0) => ResizeAnchor.Top,
            (2, 0) => ResizeAnchor.TopRight,
            (0, 1) => ResizeAnchor.Left,
            (1, 1) => ResizeAnchor.Center,
            (2, 1) => ResizeAnchor.Right,
            (0, 2) => ResizeAnchor.BottomLeft,
            (1, 2) => ResizeAnchor.Bottom,
            (2, 2) => ResizeAnchor.BottomRight,
            _ => throw new NotImplementedException()
        };
        if (_selectedToggleButton != null) _selectedToggleButton.IsChecked = false;
        _selectedToggleButton = btn;
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as ToggleButton).IsChecked.Value)
            e.Handled = true;
    }
}
