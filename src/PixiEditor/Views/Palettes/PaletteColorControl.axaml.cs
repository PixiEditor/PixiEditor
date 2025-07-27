using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.Views.Palettes;

internal partial class PaletteColorControl : UserControl
{
    public const string PaletteColorDaoFormat = "PixiEditor.PaletteColor";

    public static readonly StyledProperty<PaletteColor> ColorProperty =
        AvaloniaProperty.Register<PaletteColorControl, PaletteColor>(nameof(Color));

    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<PaletteColorControl, bool>(
        nameof(IsSelected));

    public static readonly StyledProperty<bool> IsSelectedSecondaryProperty = AvaloniaProperty.Register<PaletteColorControl, bool>(
        nameof(IsSelectedSecondary));

    public bool IsSelectedSecondary
    {
        get => GetValue(IsSelectedSecondaryProperty);
        set => SetValue(IsSelectedSecondaryProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

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

    public ICommand DropCommand
    {
        get { return (ICommand)GetValue(DropCommandProperty); }
        set { SetValue(DropCommandProperty, value); }
    }


    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<PaletteColorControl, CornerRadius>(nameof(CornerRadius), new CornerRadius(5f));

    private Point clickPoint;
    public static readonly StyledProperty<ICommand> DropCommandProperty = AvaloniaProperty.Register<PaletteColorControl, ICommand>("DropCommand");

    public PaletteColorControl()
    {
        InitializeComponent();
        
        this.AddHandler(DragDrop.DropEvent, PaletteColor_OnDrop);
    }

    private void PaletteColor_OnDrop(object? sender, DragEventArgs e)
    {
        e.Source = this;
        DropCommand?.Execute(e);
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
                try
                {
                    DataObject data = new DataObject();
                    data.Set(PaletteColorDaoFormat, colorControl.Color.ToString());
                    DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
                }
                catch
                {
                    // ignored
                }

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
