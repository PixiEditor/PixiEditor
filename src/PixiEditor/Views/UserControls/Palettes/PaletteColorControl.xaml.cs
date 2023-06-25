using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Extensions.Palettes;

namespace PixiEditor.Views.UserControls.Palettes;

internal partial class PaletteColorControl : UserControl
{
    public const string PaletteColorDaoFormat = "PixiEditor.PaletteColor";

    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(PaletteColor), typeof(PaletteColorControl), new PropertyMetadata(default(PaletteColor)));

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

    public static readonly DependencyProperty AssociatedKeyProperty =
        DependencyProperty.Register(nameof(AssociatedKey), typeof(int?), typeof(PaletteColorControl), new PropertyMetadata(null));

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }


    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(PaletteColorControl), new PropertyMetadata(new CornerRadius(5f)));

    private Point clickPoint;

    public PaletteColorControl()
    {
        InitializeComponent();
    }

    private void PaletteColor_OnMouseMove(object sender, MouseEventArgs e)
    {
        PaletteColorControl colorControl = sender as PaletteColorControl;
        if (colorControl != null && e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
        {
            var movedDistance = (clickPoint - e.GetPosition(this)).Length;
            if (movedDistance > 10)
            {
                DataObject data = new DataObject();
                data.SetData(PaletteColorControl.PaletteColorDaoFormat, colorControl.Color.ToString());
                DragDrop.DoDragDrop(colorControl, data, DragDropEffects.Move);
                e.Handled = true;
            }
        }
    }

    private void PaletteColor_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            clickPoint = e.GetPosition(this);
            CaptureMouse();
        }
    }

    private void PaletteColor_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        ReleaseMouseCapture();
    }
}
