using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using ColorPicker;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes;
using DrawieColor = Drawie.Backend.Core.ColorsImpl.Color;

namespace PixiEditor.Views.Nodes;

[TemplatePart("PART_Rectangle", typeof(Border))]
[TemplatePart("PART_ColorPicker", typeof(PortableColorPicker))]
public class StickyNoteNodeView : ResizableNodeView
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<StickyNoteNodeView, string>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<IBrush> ForegroundBrushProperty =
        AvaloniaProperty.Register<StickyNoteNodeView, IBrush>(
            nameof(ForegroundBrush), Brushes.Black);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IBrush ForegroundBrush
    {
        get => GetValue(ForegroundBrushProperty);
        set => SetValue(ForegroundBrushProperty, value);
    }

    private PortableColorPicker? _picker;
    private INodePropertyHandler? _colorHandler;
    private bool _updatingPicker;

    static StickyNoteNodeView()
    {
        NodeProperty.Changed.Subscribe(OnNodeChanged);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Parent is Visual parent)
            parent.ZIndex = 1;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _picker = e.NameScope.Find<PortableColorPicker>("PART_ColorPicker");

        if (_picker != null)
            _picker.PropertyChanged += OnPickerPropertyChanged;

        UpdateColorsFromHandler();
    }

    private static void OnNodeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not StickyNoteNodeView view) return;
        view.UpdateNodeBinding(e.OldValue as INodeHandler, e.NewValue as INodeHandler);
    }

    private void UpdateNodeBinding(INodeHandler? oldNode, INodeHandler? newNode)
    {
        if (_colorHandler != null)
        {
            _colorHandler.ValueChanged -= OnColorValueChanged;
            _colorHandler = null;
        }

        if (newNode != null &&
            newNode.InputPropertyMap.TryGetValue(StickyNoteNode.ColorPropertyName, out var handler))
        {
            _colorHandler = handler;
            _colorHandler.ValueChanged += OnColorValueChanged;
        }

        UpdateColorsFromHandler();
    }

    private void OnColorValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        UpdateColorsFromHandler();
    }

    private void UpdateColorsFromHandler()
    {
        if (_colorHandler?.Value is not DrawieColor drawie) return;

        var avalonia = drawie.ToColor();
        if (fillBrush != null) fillBrush.Color = avalonia;
        if (strokeBrush != null) strokeBrush.Color = ShadedBorder(avalonia);
        ForegroundBrush = ContrastForeground(avalonia);

        if (_picker != null)
        {
            _updatingPicker = true;
            _picker.SelectedColor = avalonia;
            _updatingPicker = false;
        }
    }

    private void OnPickerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_updatingPicker) return;
        if (e.Property != PickerControlBase.SelectedColorProperty) return;
        if (_colorHandler == null || e.NewValue is not Color newColor) return;

        _colorHandler.Value = new DrawieColor(newColor.R, newColor.G, newColor.B, newColor.A);
    }


    private static IBrush ContrastForeground(Color bg)
    {
        double lum = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255.0;
        return lum > 0.5 ? Brushes.Black : Brushes.White;
    }

    private static Color ShadedBorder(Color bg)
    {
        double lum = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255.0;
        double factor = lum > 0.5 ? 0.65 : 1.5;
        return Color.FromArgb(bg.A,
            (byte)Math.Clamp(bg.R * factor, 0, 255),
            (byte)Math.Clamp(bg.G * factor, 0, 255),
            (byte)Math.Clamp(bg.B * factor, 0, 255));
    }
}
