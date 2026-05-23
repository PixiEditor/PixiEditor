using Avalonia;
using Avalonia.Media;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using DrawieColor = Drawie.Backend.Core.ColorsImpl.Color;

namespace PixiEditor.ViewModels.Nodes;

public sealed class CommentZoneViewModel : NodeFrameViewModelBase
{
    private const double EdgeMargin = 10.0;
    private const double MinSize = 1.0;

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
        private set => SetProperty(ref borderBrush, value);
    }

    private VecD dragInitialSize;
    private VecD dragInitialOffset;
    private VecD dragStartPosition;
    private CommentZoneDragMode dragMode;
    private bool isDragging;

    public CommentZoneViewModel(Guid id, string internalName, INodeHandler comment) : base(id, [comment])
    {
        InternalName = internalName;
        Comment = comment;

        if (Comment.InputPropertyMap.TryGetValue(CommentNode.ColorPropertyName, out var colorHandler))
        {
            colorHandler.ValueChanged += OnColorChanged;
        }

        if (Comment.InputPropertyMap.TryGetValue(CommentNode.SizePropertyName, out var sizeHandler))
        {
            sizeHandler.ValueChanged += OnGeometryInputChanged;
        }

        if (Comment.InputPropertyMap.TryGetValue(CommentNode.OffsetPropertyName, out var offsetHandler))
        {
            offsetHandler.ValueChanged += OnGeometryInputChanged;
        }

        UpdateBrushesFromColor();
        CalculateBounds();
    }

    public CommentZoneDragMode HitTest(VecD graphPos)
    {
        var pos = Comment.PositionBindable;
        var offset = ReadVecInput(CommentNode.OffsetPropertyName, new VecD(0, 60));
        var size = ReadVecInput(CommentNode.SizePropertyName, new VecD(300, 200));
        var tl = pos + offset;
        var br = tl + size;

        if (graphPos.X < tl.X - EdgeMargin || graphPos.X > br.X + EdgeMargin ||
            graphPos.Y < tl.Y - EdgeMargin || graphPos.Y > br.Y + EdgeMargin)
        {
            return CommentZoneDragMode.None;
        }

        bool nearLeft = Math.Abs(graphPos.X - tl.X) <= EdgeMargin;
        bool nearRight = Math.Abs(graphPos.X - br.X) <= EdgeMargin;
        bool nearTop = Math.Abs(graphPos.Y - tl.Y) <= EdgeMargin;
        bool nearBottom = Math.Abs(graphPos.Y - br.Y) <= EdgeMargin;

        if (nearTop && nearLeft) return CommentZoneDragMode.TopLeft;
        if (nearTop && nearRight) return CommentZoneDragMode.TopRight;
        if (nearBottom && nearLeft) return CommentZoneDragMode.BottomLeft;
        if (nearBottom && nearRight) return CommentZoneDragMode.BottomRight;
        if (nearTop) return CommentZoneDragMode.Top;
        if (nearBottom) return CommentZoneDragMode.Bottom;
        if (nearLeft) return CommentZoneDragMode.Left;
        if (nearRight) return CommentZoneDragMode.Right;
        return CommentZoneDragMode.Move;
    }

    public void BeginDrag(VecD startGraphPos, CommentZoneDragMode mode)
    {
        if (mode == CommentZoneDragMode.None) return;

        dragInitialOffset = ReadVecInput(CommentNode.OffsetPropertyName, new VecD(0, 60));
        dragInitialSize = ReadVecInput(CommentNode.SizePropertyName, new VecD(300, 200));
        dragStartPosition = startGraphPos;
        dragMode = mode;
        isDragging = true;
    }

    public void UpdateDrag(VecD currentGraphPos)
    {
        if (!isDragging || Comment is not NodeViewModel commentNode) return;

        var delta = currentGraphPos - dragStartPosition;

        double top = dragInitialOffset.Y;
        double left = dragInitialOffset.X;
        double right = left + dragInitialSize.X;
        double bottom = top + dragInitialSize.Y;

        switch (dragMode)
        {
            case CommentZoneDragMode.Move:
                top += delta.Y;
                bottom += delta.Y;
                left += delta.X;
                right += delta.X;
                break;
            case CommentZoneDragMode.Top:
                top = Math.Min(top + delta.Y, bottom - MinSize);
                break;
            case CommentZoneDragMode.Bottom:
                bottom = Math.Max(top + MinSize, bottom + delta.Y);
                break;
            case CommentZoneDragMode.Left:
                left = Math.Min(left + delta.X, right - MinSize);
                break;
            case CommentZoneDragMode.Right:
                right = Math.Max(left + MinSize, right + delta.X);
                break;
            case CommentZoneDragMode.TopLeft:
                top = Math.Min(top + delta.Y, bottom - MinSize);
                left = Math.Min(left + delta.X, right - MinSize);
                break;
            case CommentZoneDragMode.TopRight:
                top = Math.Min(top + delta.Y, bottom - MinSize);
                right = Math.Max(left + MinSize, right + delta.X);
                break;
            case CommentZoneDragMode.BottomLeft:
                bottom = Math.Max(top + MinSize, bottom + delta.Y);
                left = Math.Min(left + delta.X, right - MinSize);
                break;
            case CommentZoneDragMode.BottomRight:
                bottom = Math.Max(top + MinSize, bottom + delta.Y);
                right = Math.Max(left + MinSize, right + delta.X);
                break;
        }

        var sizeVec = new VecI((int)Math.Round(right - left), (int)Math.Round(bottom - top));
        var offsetVec = new VecI((int)Math.Round(left), (int)Math.Round(top));

        commentNode.Internals.ActionAccumulator.AddActions(
            new CommentZoneRect_Action(Comment.Id, sizeVec, offsetVec));
    }

    public void EndDrag()
    {
        if (!isDragging || Comment is not NodeViewModel commentNode) return;
        isDragging = false;
        commentNode.Internals.ActionAccumulator.AddFinishedActions(new EndCommentZoneRect_Action());
    }

    protected override void CalculateBounds()
    {
        if (Comment == null) return;

        var pos = Comment.PositionBindable;
        var size = ReadVecInput(CommentNode.SizePropertyName, new VecD(300, 200));
        var offset = ReadVecInput(CommentNode.OffsetPropertyName, new VecD(0, 60));

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
        if (!Comment.InputPropertyMap.TryGetValue(CommentNode.ColorPropertyName, out var handler) || handler.Value is not DrawieColor c)
        {
            return;
        }

        var fillColor = new Avalonia.Media.Color((byte)(c.A / 2), c.R, c.G, c.B);
        var strokeColor = new Avalonia.Media.Color(c.A, c.R, c.G, c.B);

        BackgroundBrush = new SolidColorBrush(fillColor);
        BorderBrush = new SolidColorBrush(strokeColor);
    }

    private VecD ReadVecInput(string propertyName, VecD fallback)
    {
        if (!Comment.InputPropertyMap.TryGetValue(propertyName, out var handler))
            return fallback;

        return handler.Value switch
        {
            VecD vd => vd,
            VecI vi => new VecD(vi.X, vi.Y),
            _ => fallback
        };
    }
}
