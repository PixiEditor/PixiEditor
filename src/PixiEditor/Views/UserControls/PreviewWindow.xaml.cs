using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels;

namespace PixiEditor.Views.UserControls;

/// <summary>
/// Interaction logic for PreviewWindow.xaml
/// </summary>
internal partial class PreviewWindow : UserControl
{
    public static readonly DependencyProperty DocumentProperty =
        DependencyProperty.Register(nameof(Document), typeof(Document), typeof(PreviewWindow));

    public Document Document
    {
        get => (Document)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public static readonly DependencyProperty ColorCursorPositionProperty =
        DependencyProperty.Register(nameof(ColorCursorPosition), typeof(Thickness), typeof(PreviewWindow));

    public Thickness ColorCursorPosition
    {
        get => (Thickness)GetValue(ColorCursorPositionProperty);
        private set => SetValue(ColorCursorPositionProperty, value);
    }

    public static readonly DependencyProperty ColorCursorColorProperty =
        DependencyProperty.Register(nameof(ColorCursorColor), typeof(Color), typeof(PreviewWindow));

    public Color ColorCursorColor
    {
        get => (Color)GetValue(ColorCursorColorProperty);
        set => SetValue(ColorCursorColorProperty, value);
    }

    public static readonly DependencyProperty PrimaryColorProperty =
        DependencyProperty.Register(nameof(PrimaryColor), typeof(Color), typeof(PreviewWindow));

    public Color PrimaryColor
    {
        get => (Color)GetValue(PrimaryColorProperty);
        set => SetValue(PrimaryColorProperty, value);
    }

    public static readonly DependencyProperty OptionsOpenProperty =
        DependencyProperty.Register(nameof(OptionsOpen), typeof(bool), typeof(PreviewWindow));

    public bool OptionsOpen
    {
        get => (bool)GetValue(OptionsOpenProperty);
        set => SetValue(OptionsOpenProperty, value);
    }

    public PreviewWindow()
    {
        InitializeComponent();

        /*imageGrid.MouseMove += ImageGrid_MouseMove;
        imageGrid.MouseRightButtonDown += ImageGrid_MouseRightButtonDown;
        imageGrid.MouseEnter += ImageGrid_MouseEnter;
        imageGrid.MouseLeave += ImageGrid_MouseLeave;*/
    }

    private void ImageGrid_MouseLeave(object sender, MouseEventArgs e)
    {
        if (ViewModelMain.Current != null)
        {
            ViewModelMain.Current.OverrideActionDisplay = false;
        }
    }

    private void ImageGrid_MouseEnter(object sender, MouseEventArgs e)
    {
        if (ViewModelMain.Current != null)
        {
            ViewModelMain.Current.ActionDisplay = "Right-click to pick color, Shift-right-click to copy color to clipboard";
            ViewModelMain.Current.OverrideActionDisplay = true;
        }
    }

    private void ImageGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftShift))
        {
            CopyColorToClipboard();
        }
        else
        {
            CopyColorToPrimary();
        }
    }

    private void CopyColorToPrimary()
    {
        PrimaryColor = ColorCursorColor;
    }

    private void CopyColorToClipboard()
    {
        if (ColorCursorColor.A == 255)
        {
            Clipboard.SetText(string.Format("#{0:X2}{1:X2}{2:X2}", ColorCursorColor.R, ColorCursorColor.G, ColorCursorColor.B));
        }
        else
        {
            Clipboard.SetText(ColorCursorColor.ToString());
        }
    }

    private void ImageGrid_MouseMove(object sender, MouseEventArgs e)
    {
        /*
        if (Document == null)
        {
            return;
        }

        Point mousePos = e.GetPosition(imageGrid);

        int x = (int)mousePos.X;
        int y = (int)mousePos.Y;

        Thickness newPos = new Thickness(x, y, 0, 0);

        if (ColorCursorPosition == newPos)
        {
            return;
        }

        ColorCursorPosition = newPos;

        var color = Document.GetColorAtPoint(x, y);
        ColorCursorColor = Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        */
    }
}
