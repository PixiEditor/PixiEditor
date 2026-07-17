using Avalonia;
using Avalonia.Controls;
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

public class CommentNodeView : ResizableNodeView
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<CommentNodeView, string>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IBrush> ForegroundBrushProperty =
        AvaloniaProperty.Register<CommentNodeView, IBrush>(
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

    private PortableColorPicker? picker;

    private INodePropertyHandler? colorHandler;
    private bool updatingPicker;

    static CommentNodeView()
    {
        NodeProperty.Changed.AddClassHandler<CommentNodeView>((view, e) =>
            view.OnNodeChanged(e.OldValue as INodeHandler, e.NewValue as INodeHandler));
    }

    public CommentNodeView()
    {
        FillOpacity = 0.5;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Parent is Visual parent)
            parent.ZIndex = -1;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        picker = e.NameScope.Find<PortableColorPicker>("PART_ColorPicker");

        if (picker != null)
            picker.PropertyChanged += OnPickerPropertyChanged;

        UpdateColorsFromHandler();
    }

    private void OnNodeChanged(INodeHandler? oldNode, INodeHandler? newNode)
    {
        if (colorHandler != null)
        {
            colorHandler.ValueChanged -= OnColorValueChanged;
            colorHandler = null;
        }

        if (newNode != null &&
            newNode.InputPropertyMap.TryGetValue(CommentNode.ColorPropertyName, out var handler))
        {
            colorHandler = handler;
            colorHandler.ValueChanged += OnColorValueChanged;
        }

        UpdateColorsFromHandler();
    }

    private void OnColorValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        UpdateColorsFromHandler();
    }

    private void UpdateColorsFromHandler()
    {
        if (colorHandler?.Value is not DrawieColor drawie) return;

        var avalonia = drawie.ToColor();
        if (fillBrush != null) fillBrush.Color = avalonia;
        if (strokeBrush != null) strokeBrush.Color = avalonia;
        ForegroundBrush = ContrastForeground(avalonia);

        if (picker != null)
        {
            updatingPicker = true;
            picker.SelectedColor = avalonia;
            updatingPicker = false;
        }
    }

    private void OnPickerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (updatingPicker) return;
        if (e.Property != PickerControlBase.SelectedColorProperty) return;
        if (colorHandler == null || e.NewValue is not Color newColor) return;

        colorHandler.Value = new DrawieColor(newColor.R, newColor.G, newColor.B, newColor.A);
    }

    private static IBrush ContrastForeground(Color bg)
    {
        double lum = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255.0;
        return lum > 0.5 ? Brushes.Black : Brushes.White;
    }
}
