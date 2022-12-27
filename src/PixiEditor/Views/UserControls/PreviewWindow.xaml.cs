using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BackendColor = PixiEditor.DrawingApi.Core.ColorsImpl.Color;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Controllers;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Models.Enums;

namespace PixiEditor.Views.UserControls;

internal partial class PreviewWindow : UserControl
{
    public static readonly DependencyProperty DocumentProperty =
        DependencyProperty.Register(nameof(Document), typeof(DocumentViewModel), typeof(PreviewWindow));

    public DocumentViewModel Document
    {
        get => (DocumentViewModel)GetValue(DocumentProperty);
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
    
    private MouseUpdateController mouseUpdateController;

    public PreviewWindow()
    {
        InitializeComponent();
        
        mouseUpdateController = new MouseUpdateController(imageGrid, ImageGrid_MouseMove);
        
        imageGrid.MouseRightButtonDown += ImageGrid_MouseRightButtonDown;
        imageGrid.MouseEnter += ImageGrid_MouseEnter;
        imageGrid.MouseLeave += ImageGrid_MouseLeave;
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
        if ((string)formatButton.ActiveItem.Value == "HEX")
        {
            Clipboard.SetText(ColorCursorColor.A == 255
                ? $"#{ColorCursorColor.R:X2}{ColorCursorColor.G:X2}{ColorCursorColor.B:X2}"
                : ColorCursorColor.ToString());
        }
        else
        {
            Clipboard.SetText(ColorCursorColor.A == 255
                ? $"rgb({ColorCursorColor.R},{ColorCursorColor.G},{ColorCursorColor.B})"
                : $"rgba({ColorCursorColor.R},{ColorCursorColor.G},{ColorCursorColor.B},{ColorCursorColor.A})");
        }
    }

    private void ImageGrid_MouseMove(object sender, MouseEventArgs e)
    {
        if (Document is null)
        {
            return;
        }

        Point mousePos = e.GetPosition(viewport);
        VecD mousePosConverted =
            new VecD(mousePos.X, mousePos.Y)
                .Divide(new VecD(viewport.ActualWidth, viewport.ActualHeight))
                .Multiply(Document.SizeBindable);

        int x = (int)mousePosConverted.X;
        int y = (int)mousePosConverted.Y;

        Thickness newPos = new Thickness(x, y, 0, 0);

        if (ColorCursorPosition == newPos)
        {
            return;
        }

        ColorCursorPosition = newPos;

        BackendColor color = Document.PickColor(new(x, y), DocumentScope.AllLayers, false, true);
        ColorCursorColor = Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
