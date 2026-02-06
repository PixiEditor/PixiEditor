using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.Models.BrushEngine;
using PixiEditor.ViewModels.BrushSystem;

namespace PixiEditor.Views.Input;

internal partial class BrushItem : UserControl
{
    public static readonly StyledProperty<BrushViewModel> BrushProperty =
        AvaloniaProperty.Register<BrushItem, BrushViewModel>("Brush");

    public static readonly StyledProperty<Texture> DrawingStrokeTextureProperty =
        AvaloniaProperty.Register<BrushItem, Texture>(
            nameof(DrawingStrokeTexture));

    public Texture DrawingStrokeTexture
    {
        get => GetValue(DrawingStrokeTextureProperty);
        set => SetValue(DrawingStrokeTextureProperty, value);
    }

    public BrushViewModel Brush
    {
        get { return (BrushViewModel)GetValue(BrushProperty); }
        set { SetValue(BrushProperty, value); }
    }

    private bool isPreviewingStroke;

    private ChunkyImage previewImage;
    private IDisposable previewTimer;
    private IEnumerator enumerator;

    private Texture previewTexture;

    static BrushItem()
    {
        BrushProperty.Changed.AddClassHandler<BrushItem>((x, e) =>
        {
            if (e.OldValue is BrushViewModel oldBrush)
            {
                oldBrush.RenderingPreviewFinished -= x.OnBrushRendered;
            }

            x.StopStrokePreviewLoop();
            var brush = e.NewValue as BrushViewModel;
            if (brush != null)
            {
                x.DrawingStrokeTexture = brush.DrawingStrokeTexture;
                brush.RenderingPreviewFinished += x.OnBrushRendered;
            }

            x.PseudoClasses.Set(":favourite", brush?.IsFavourite ?? false);
        });
    }

    public BrushItem()
    {
        InitializeComponent();
    }

    private void OnBrushRendered()
    {
        if (!isPreviewingStroke)
        {
            DrawingStrokeTexture = Brush?.DrawingStrokeTexture;
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        StartStrokePreviewLoop();
    }

    public void ToggleFavorite()
    {
        var brush = Brush;
        if (brush == null)
            return;

        brush.IsFavourite = !brush.IsFavourite;
        PseudoClasses.Set(":favourite", brush.IsFavourite);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        StopStrokePreviewLoop();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopStrokePreviewLoop();
        previewTexture?.Dispose();
        previewTexture = null;

        previewImage?.Dispose();
        previewImage = null;
    }

    private void StartStrokePreviewLoop()
    {
        if (isPreviewingStroke)
            return;

        var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
        BrushOutputNode? brushNode =
            Brush?.Brush?.Document?.AccessInternalReadOnlyDocument().NodeGraph
                .LookupNode(Brush?.Brush?.OutputNodeId ?? Guid.Empty) as BrushOutputNode;
        if (brushNode == null)
        {
            ctx.Dispose();
            return;
        }

        if (previewTexture == null ||
            previewTexture.Size.X != BrushOutputNode.StrokePreviewSizeX ||
            previewTexture.Size.Y != BrushOutputNode.StrokePreviewSizeY)
        {
            previewTexture?.Dispose();
            previewTexture =
                Texture.ForDisplay(new VecI(BrushOutputNode.StrokePreviewSizeX, BrushOutputNode.StrokePreviewSizeY));
        }

        if (previewImage == null ||
            previewImage.CommittedSize.X != BrushOutputNode.StrokePreviewSizeX ||
            previewImage.CommittedSize.Y != BrushOutputNode.StrokePreviewSizeY)
        {
            previewImage?.Dispose();
            previewImage =
                new ChunkyImage(new VecI(BrushOutputNode.StrokePreviewSizeX, BrushOutputNode.StrokePreviewSizeY),
                    ColorSpace.CreateSrgb());
        }

        DrawingStrokeTexture = previewTexture;

        previewTexture.DrawingSurface.Canvas.Clear();
        previewImage.EnqueueClear();
        previewImage.CommitChanges();
        enumerator = brushNode.DrawStrokePreviewEnumerable(previewImage, CreateContext(),
            BrushOutputNode.StrokePreviewSizeY / 2,
            new VecD(0, BrushOutputNode.YOffsetInPreview)).GetEnumerator();

        previewTexture.DrawingSurface.Canvas.Clear();

        ctx.Dispose();
        previewTimer = DispatcherTimer.Run(() =>
        {
            if ((brushNode != null && previewTexture != null) && (brushNode.IsDisposed || previewTexture.IsDisposed))
            {
                StopStrokePreviewLoop();
                return false;
            }

            try
            {
                var innerCtx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
                if (!enumerator.MoveNext())
                {
                    innerCtx.Dispose();
                    DispatcherTimer.RunOnce(() =>
                    {
                        if (isPreviewingStroke)
                        {
                            StopStrokePreviewLoop();
                            StartStrokePreviewLoop();
                            isPreviewingStroke = true;
                        }
                    }, TimeSpan.FromSeconds(1));
                    return false;
                }

                using Paint srcOver = new() { BlendMode = BlendMode.Src, Style = PaintStyle.Fill };
                previewImage.DrawMostUpToDateRegionOn(
                    new RectI(0, 0, previewImage.CommittedSize.X, previewImage.CommittedSize.Y),
                    ChunkResolution.Full,
                    previewTexture.DrawingSurface.Canvas,
                    VecI.Zero, srcOver);

                innerCtx.Dispose();
                StrokePreviewControl.QueueNextFrame();
            }
            catch
            {
                StopStrokePreviewLoop();
                return false;
            }

            return isPreviewingStroke;
        }, TimeSpan.FromMilliseconds(8));

        isPreviewingStroke = true;
    }

    private RenderContext CreateContext()
    {
        return new RenderContext(
            previewTexture.DrawingSurface.Canvas,
            0,
            ChunkResolution.Full,
            previewTexture.Size,
            previewTexture.Size,
            ColorSpace.CreateSrgb(),
            SamplingOptions.Bilinear,
            Brush?.Brush?.Document.AccessInternalReadOnlyDocument().NodeGraph);
    }

    private void StopStrokePreviewLoop()
    {
        if (!isPreviewingStroke)
            return;

        isPreviewingStroke = false;
        previewTimer?.Dispose();
        if (enumerator is IDisposable disposable)
        {
            disposable.Dispose();
        }

        DrawingStrokeTexture = Brush?.DrawingStrokeTexture;
    }
}
