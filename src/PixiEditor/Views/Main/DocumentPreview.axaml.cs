using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Document;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Main;

internal partial class DocumentPreview : UserControl
{
    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<DocumentPreview, DocumentViewModel>(nameof(Document));

    public static readonly StyledProperty<Color> ColorCursorColorProperty =
        AvaloniaProperty.Register<DocumentPreview, Color>(nameof(ColorCursorColor));

    public static readonly StyledProperty<Color> PrimaryColorProperty =
        AvaloniaProperty.Register<DocumentPreview, Color>(nameof(PrimaryColor));

    public static readonly StyledProperty<int> MaxBilinearSamplingSizeProperty = AvaloniaProperty.Register<DocumentPreview, int>(
        nameof(MaxBilinearSamplingSize), 4096);

    public int MaxBilinearSamplingSize
    {
        get => GetValue(MaxBilinearSamplingSizeProperty);
        set => SetValue(MaxBilinearSamplingSizeProperty, value);
    }
    public DocumentViewModel Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public Color ColorCursorColor
    {
        get => GetValue(ColorCursorColorProperty);
        set => SetValue(ColorCursorColorProperty, value);
    }

    public Color PrimaryColor
    {
        get => GetValue(PrimaryColorProperty);
        set => SetValue(PrimaryColorProperty, value);
    }

    public static readonly StyledProperty<VecI> ColorCursorPositionProperty = AvaloniaProperty.Register<DocumentPreview, VecI>(
        nameof(ColorCursorPosition));

    public VecI ColorCursorPosition
    {
        get => GetValue(ColorCursorPositionProperty);
        set => SetValue(ColorCursorPositionProperty, value);
    }
    
    private MouseUpdateController mouseUpdateController;

    public DocumentPreview()
    {
        InitializeComponent();
        
        imageGrid.PointerPressed += ImageGrid_MouseRightButtonDown;
        imageGrid.PointerEntered += ImageGrid_MouseEnter;
        imageGrid.PointerExited += ImageGrid_MouseLeave;
        
        imageGrid.Loaded += OnGridLoaded;
        imageGrid.Unloaded += OnGridUnloaded;
    }

    private void OnGridUnloaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController?.Dispose();
    }

    private void OnGridLoaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController = new MouseUpdateController(imageGrid, ImageGrid_MouseMove);
    }

    private void ImageGrid_MouseLeave(object sender, PointerEventArgs e)
    {
        if (ViewModelMain.Current != null)
        {
            ViewModelMain.Current.ActionDisplays[nameof(DocumentPreview)] = null;
        }
    }

    private void ImageGrid_MouseEnter(object sender, PointerEventArgs e)
    {
        if (ViewModelMain.Current != null)
        {
            ViewModelMain.Current.ActionDisplays[nameof(DocumentPreview)] = "NAVIGATOR_PICK_ACTION_DISPLAY";
        }
    }

    private async void ImageGrid_MouseRightButtonDown(object sender, PointerPressedEventArgs e)
    {
        if(e.GetMouseButton(this) != MouseButton.Right) return;
        
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            await CopyColorToClipboard();
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

    private async Task CopyColorToClipboard()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null || topLevel.Clipboard == null)
        {
            return;
        }

        if ((string)formatButton.ActiveItem.Value == "HEX")
        {
            await topLevel.Clipboard.SetTextAsync(ColorCursorColor.A == 255
                ? $"#{ColorCursorColor.R:X2}{ColorCursorColor.G:X2}{ColorCursorColor.B:X2}"
                : ColorCursorColor.ToString());
        }
        else
        {
            await topLevel.Clipboard.SetTextAsync(ColorCursorColor.A == 255
                ? $"rgb({ColorCursorColor.R},{ColorCursorColor.G},{ColorCursorColor.B})"
                : $"rgba({ColorCursorColor.R},{ColorCursorColor.G},{ColorCursorColor.B},{ColorCursorColor.A})");
        }
    }

    private void ImageGrid_MouseMove(PointerEventArgs e)
    {
        if (Document is null)
        {
            return;
        }

        Point mousePos = e.GetPosition(viewport);
        VecD mousePosConverted =
            new VecD(mousePos.X, mousePos.Y)
                .Divide(new VecD(viewport.Bounds.Width, viewport.Bounds.Height))
                .Multiply(Document.SizeBindable);

        int x = (int)mousePosConverted.X;
        int y = (int)mousePosConverted.Y;

        if (x < 0 || x > Document.Width || y < 0 || y > Document.Height)
            return;
        
        if (x == ColorCursorPosition.X && y == ColorCursorPosition.Y)
        {
            return;
        }

        ColorCursorPosition = new VecI(x, y);
        
        var color = Document.PickColor(new(x, y), DocumentScope.Canvas, false, true, Document.AnimationDataViewModel.ActiveFrameBindable);
        ColorCursorColor = Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}

