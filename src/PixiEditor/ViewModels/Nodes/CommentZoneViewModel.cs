using Avalonia;
using Avalonia.Media;
using Drawie.Numerics;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using DrawieColor = Drawie.Backend.Core.ColorsImpl.Color;

namespace PixiEditor.ViewModels.Nodes;

public sealed class CommentZoneViewModel : NodeFrameViewModelBase
{
    private const string SizePropName = "Size";
    private const string OffsetPropName = "Offset";
    private const string ColorPropName = "Color";

    public INodeHandler Comment { get; }
    private IBrush backgroundBrush = Brushes.Transparent;
    public IBrush BackgroundBrush
    {
        get => backgroundBrush;
        private set => SetProperty(ref backgroundBrush, value);
    }
    private IBrush borderBrush = Brushes.Transparent;
    public IBrush BorderBrush
    {
        get => borderBrush;
        set => SetProperty(ref borderBrush, value);
    }
    public CommentZoneViewModel(Guid id, string internalName, INodeHandler comment) : base(id, [comment])
    {
        InternalName = internalName;
        Comment = comment;

        if (Comment.InputPropertyMap.TryGetValue(ColorPropName, out var colorHandler))
        {
            colorHandler.ValueChanged += OnColorChanged;
        }

        if (Comment.InputPropertyMap.TryGetValue(SizePropName, out var sizeHandler))
        {
            sizeHandler.ValueChanged += OnGeometryInputChanged;
        }

        if (Comment.InputPropertyMap.TryGetValue(OffsetPropName, out var offsetHandler))
        {
            offsetHandler.ValueChanged += OnGeometryInputChanged;
        }

        UpdateBrushesFromColor();
        CalculateBounds();
    }

    private void OnColorChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        UpdateBrushesFromColor();
    }

    private void OnGeometryInputChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        CalculateBounds();
    }

    private void UpdateBrushesFromColor()
    {
        if (!Comment.InputPropertyMap.TryGetValue(ColorPropName, out var handler) || handler.Value is not DrawieColor c)
        {
            return;
        }

        var fillColor = new Avalonia.Media.Color((byte)(c.A / 2), c.R, c.G, c.B);
        var strokeColor = new Avalonia.Media.Color(c.A, c.R, c.G, c.B);

        BackgroundBrush = new SolidColorBrush(fillColor);
        BorderBrush = new SolidColorBrush(strokeColor);
    }

    protected override void CalculateBounds()
    {
        if (Comment == null) return;

        var pos = Comment.PositionBindable;
        var size = ReadVecInput(SizePropName, new VecD(300, 200));
        var offset = ReadVecInput(OffsetPropName, new VecD(0, 60));

        var tl = pos + offset;
        var br = tl + size;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(tl.X, tl.Y), isFilled: true);
            ctx.LineTo(new Point(br.X, tl.Y));
            ctx.LineTo(new Point(br.X, br.Y));
            ctx.LineTo(new Point(tl.X, br.Y));
            ctx.EndFigure(isClosed: true);
        }

        Geometry = geometry;
    }

    private VecD ReadVecInput(string propertyName, VecD fallback)
    {
        if (!Comment.InputPropertyMap.TryGetValue(propertyName, out var handler))
            return fallback;

        return handler.Value switch
        {
            VecD vd => vd,
            VecI vi => new VecD(vi.X, vi.Y),
            _ => fallback,
        };
    }
}
