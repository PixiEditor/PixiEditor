using Avalonia;
using Avalonia.Input;

namespace PixiEditor.AvaloniaUI.Views.Dialogs.Debugging;

public partial class PointerDebugPopup : PixiEditorPopup
{
    public static readonly StyledProperty<PointerPointProperties> LastPointProperty =
        AvaloniaProperty.Register<PointerDebugPopup, PointerPointProperties>(nameof(LastPoint));

    public PointerPointProperties LastPoint
    {
        get => GetValue(LastPointProperty);
        set => SetValue(LastPointProperty, value);
    }
    
    public static readonly StyledProperty<PointerType> PointerTypeProperty =
        AvaloniaProperty.Register<PointerDebugPopup, PointerType>(nameof(LastPoint));

    public PointerType PointerType
    {
        get => GetValue(PointerTypeProperty);
        set => SetValue(PointerTypeProperty, value);
    }
    
    public PointerDebugPopup()
    {
        InitializeComponent();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(DebugField);
        LastPoint = point.Properties;
        
        DebugField.ReportPoint(point);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        DebugField.ClearPoints();
    }
}
