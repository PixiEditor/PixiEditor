using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.AvaloniaUI.Views.Palettes;

internal partial class PaletteColorControl : UserControl
{
    public const string PaletteColorDaoFormat = "PixiEditor.PaletteColor";

    public static readonly StyledProperty<PaletteColor> ColorProperty =
        AvaloniaProperty.Register<PaletteColorControl, PaletteColor>(nameof(Color));

    public PaletteColor Color
    {
        get { return (PaletteColor)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }

    public int? AssociatedKey
    {
        get { return (int?)GetValue(AssociatedKeyProperty); }
        set { SetValue(AssociatedKeyProperty, value); }
    }

    public static readonly StyledProperty<int?> AssociatedKeyProperty =
        AvaloniaProperty.Register<PaletteColorControl, int?>(nameof(AssociatedKey));

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }


    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<PaletteColorControl, CornerRadius>(nameof(CornerRadius), new CornerRadius(5f));

    private Point clickPoint;

    public PaletteColorControl()
    {
        InitializeComponent();
    }

    private void PaletteColor_OnMouseMove(object? sender, PointerEventArgs e)
    {
        PaletteColorControl colorControl = sender as PaletteColorControl;

        bool isLeftButtonPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;

        if (colorControl != null && isLeftButtonPressed && e.Pointer.Captured == this)
        {
            var movedDistance = (clickPoint - e.GetPosition(this));
            float length = (float)Math.Sqrt(movedDistance.X * movedDistance.X + movedDistance.Y * movedDistance.Y);
            if (length > 10)
            {
                DataObject data = new DataObject();
                data.Set(PaletteColorDaoFormat, colorControl.Color.ToString());
                DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
                e.Handled = true;
            }
        }
    }

    private void PaletteColor_OnMouseDown(object? sender, PointerPressedEventArgs e)
    {
        var leftPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        if (leftPressed)
        {
            clickPoint = e.GetPosition(this);
            e.Pointer.Capture(this);
        }
    }

    private void PaletteColor_OnMouseUp(object? sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
    }
}
