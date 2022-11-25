using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.Views.UserControls.Palettes;

internal partial class PaletteColor : UserControl
{
    public const string PaletteColorDaoFormat = "PixiEditor.PaletteColor";

    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(PaletteColor), new PropertyMetadata(default(Color)));

    public Color Color
    {
        get { return (Color)GetValue(ColorProperty); }
        set { SetValue(ColorProperty, value); }
    }


    public int? AssociatedKey
    {
        get { return (int?)GetValue(AssociatedKeyProperty); }
        set { SetValue(AssociatedKeyProperty, value); }
    }


    public static readonly DependencyProperty AssociatedKeyProperty =
        DependencyProperty.Register(nameof(AssociatedKey), typeof(int?), typeof(PaletteColor), new PropertyMetadata(null));

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }


    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(PaletteColor), new PropertyMetadata(new CornerRadius(5f)));

    private Point clickPoint;

    public PaletteColor()
    {
        InitializeComponent();
    }

    private void PaletteColor_OnMouseMove(object sender, MouseEventArgs e)
    {
        PaletteColor color = sender as PaletteColor;
        if (color != null && e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
        {
            var movedDistance = (clickPoint - e.GetPosition(this)).Length;
            if (movedDistance > 10)
            {
                DataObject data = new DataObject();
                data.SetData(PaletteColor.PaletteColorDaoFormat, color.Color.ToString());
                DragDrop.DoDragDrop(color, data, DragDropEffects.Move);
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
