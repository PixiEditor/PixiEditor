using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.VisualTree;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Animations;

[TemplatePart("PART_ResizePanelRight", typeof(InputElement))]
[TemplatePart("PART_ResizePanelLeft", typeof(InputElement))]
internal class KeyFrame : TemplatedControl
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
        
        _resizePanelLeft.PointerCaptureLost += UpdateKeyFrame;
        _resizePanelRight.PointerCaptureLost += UpdateKeyFrame;

        PointerPressed += CapturePointer;
        PointerMoved += DragOnPointerMoved;
        PointerCaptureLost += UpdateKeyFrame;

        if (Item is not KeyFrameGroupViewModel)
        {
            MultiBinding marginBinding = new MultiBinding
            {
                Converter = new DurationToMarginConverter(),
                Bindings =
                {
                    new Binding("StartFrameBindable") { Source = Item }, new Binding("Scale") { Source = this },
                },
            };

            ContentPresenter contentPresenter = this.FindAncestorOfType<ContentPresenter>();
            contentPresenter.Bind(MarginProperty, marginBinding);
        }
    }
    
    private void CapturePointer(object? sender, PointerPressedEventArgs e)
    {
        if (Item is null || e.Handled)
        {
            return;
        }
        
        e.Pointer.Capture(sender as IInputElement);
        clickFrameOffset = Item.StartFrameBindable - (int)Math.Floor(e.GetPosition(this.FindAncestorOfType<Grid>()).X / Scale);
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
            Item.ChangeFrameLength(Item.StartFrameBindable, MousePosToFrame(e) - Item.StartFrameBindable);
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
            
            if (frame >= Item.StartFrameBindable + Item.DurationBindable)
            {
                frame = Item.StartFrameBindable + Item.DurationBindable - 1;
            }
            
            int oldStartFrame = Item.StartFrameBindable;
            Item.ChangeFrameLength(frame, Item.DurationBindable + oldStartFrame - frame);
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
            Item.ChangeFrameLength(frame + clickFrameOffset, Item.DurationBindable);
        }
    }

    private int MousePosToFrame(PointerEventArgs e, bool round = true)
    {
        double x = e.GetPosition(this.FindAncestorOfType<Grid>()).X;
        int frame;
        if (round)
        {
            frame = (int)Math.Round(x / Scale);
        }
        else
        {
            frame = (int)Math.Floor(x / Scale);
        }
        
        frame = Math.Max(0, frame);
        return frame;
    }
    
    private void UpdateKeyFrame(object? sender, PointerCaptureLostEventArgs e)
    {
        if (Item is null || e.Source is not KeyFrame)
        {
            return;
        }
        
        Item.EndChangeFrameLength();
    }
}
