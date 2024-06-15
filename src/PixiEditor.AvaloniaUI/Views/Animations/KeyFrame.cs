using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Animations;

[TemplatePart("PART_ResizePanelRight", typeof(InputElement))]
[TemplatePart("PART_ResizePanelLeft", typeof(InputElement))]
public class KeyFrame : TemplatedControl
{
    public static readonly StyledProperty<KeyFrameViewModel> ItemProperty = AvaloniaProperty.Register<KeyFrame, KeyFrameViewModel>(
        nameof(Item));

    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<KeyFrame, double>(nameof(Scale), 100);

    public KeyFrameViewModel Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public double Scale
    {
        get { return (double)GetValue(ScaleProperty); }
        set { SetValue(ScaleProperty, value); }
    }
    
    private InputElement _resizePanelRight;
    private InputElement _resizePanelLeft;

    private int clickFrameOffset;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _resizePanelRight = e.NameScope.Find<InputElement>("PART_ResizePanelRight");
        _resizePanelLeft = e.NameScope.Find<InputElement>("PART_ResizePanelLeft");

        _resizePanelRight.PointerPressed += CapturePointer;
        _resizePanelRight.PointerMoved += ResizePanelRightOnPointerMoved;

        _resizePanelLeft.PointerPressed += CapturePointer;
        _resizePanelLeft.PointerMoved += ResizePanelLeftOnPointerMoved;

        PointerPressed += CapturePointer;
        PointerMoved += DragOnPointerMoved;
    }
    
    private void CapturePointer(object? sender, PointerPressedEventArgs e)
    {
        if (Item is null || e.Handled)
        {
            return;
        }
        
        e.Pointer.Capture(sender as IInputElement);
        clickFrameOffset = Item.StartFrame - (int)Math.Floor(e.GetPosition(this.FindAncestorOfType<Canvas>()).X / Scale);
        e.Handled = true;
    }

    private void ResizePanelRightOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (Item is null)
        {
            return;
        }
        
        if (e.GetCurrentPoint(_resizePanelRight).Properties.IsLeftButtonPressed)
        {
            Item.Duration = MousePosToFrame(e) - Item.StartFrame;
        }
        
        e.Handled = true;
    }
    
    private void ResizePanelLeftOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (Item is null)
        {
            return;
        }
        
        if (e.GetCurrentPoint(_resizePanelLeft).Properties.IsLeftButtonPressed)
        {
            int frame = MousePosToFrame(e);
            
            
            if (frame >= Item.StartFrame + Item.Duration)
            {
                frame = Item.StartFrame + Item.Duration - 1;
            }
            
            int oldStartFrame = Item.StartFrame;
            Item.StartFrame = frame;
            Item.Duration += oldStartFrame - Item.StartFrame;
        }
        
        e.Handled = true;
    }
    
    private void DragOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (Item is null)
        {
            return;
        }
        
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var frame = MousePosToFrame(e, false);
            Item.StartFrame = frame + clickFrameOffset;
        }
    }

    private int MousePosToFrame(PointerEventArgs e, bool round = true)
    {
        double x = e.GetPosition(this.FindAncestorOfType<Canvas>()).X;
        int frame;
        if (round)
        {
            frame = (int)Math.Round(x / Scale);
        }
        else
        {
            frame = (int)Math.Floor(x / Scale);
        }
        
        return frame;
    }
}
