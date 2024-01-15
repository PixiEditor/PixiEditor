using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Hardware.Info;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.AvaloniaUI.ViewModels;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Main;

internal partial class Navigation : UserControl
{
    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<Navigation, DocumentViewModel>(nameof(Document));

    public static readonly StyledProperty<Thickness> ColorCursorPositionProperty =
        AvaloniaProperty.Register<Navigation, Thickness>(nameof(ColorCursorPosition));

    public static readonly StyledProperty<Color> ColorCursorColorProperty =
        AvaloniaProperty.Register<Navigation, Color>(nameof(ColorCursorColor));

    public static readonly StyledProperty<Color> PrimaryColorProperty =
        AvaloniaProperty.Register<Navigation, Color>(nameof(PrimaryColor));

    public DocumentViewModel Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public Thickness ColorCursorPosition
    {
        get => GetValue(ColorCursorPositionProperty);
        private set => SetValue(ColorCursorPositionProperty, value);
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
    
    private MouseUpdateController mouseUpdateController;

    public Navigation()
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
            ViewModelMain.Current.ActionDisplays[nameof(Navigation)] = null;
        }
    }

    private void ImageGrid_MouseEnter(object sender, PointerEventArgs e)
    {
        if (ViewModelMain.Current != null)
        {
            ViewModelMain.Current.ActionDisplays[nameof(Navigation)] = "NAVIGATOR_PICK_ACTION_DISPLAY";
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

        Thickness newPos = new Thickness(x, y, 0, 0);

        if (ColorCursorPosition == newPos)
        {
            return;
        }

        ColorCursorPosition = newPos;

        var color = Document.PickColor(new(x, y), DocumentScope.AllLayers, false, true);
        ColorCursorColor = Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}

