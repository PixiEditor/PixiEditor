using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.Models.BrushEngine;

namespace PixiEditor.Views.Input;

internal partial class BrushItem : UserControl
{
    public static readonly StyledProperty<Brush> BrushProperty = AvaloniaProperty.Register<BrushItem, Brush>("Brush");

    public static readonly StyledProperty<Texture> DrawingStrokeTextureProperty =
        AvaloniaProperty.Register<BrushItem, Texture>(
            nameof(DrawingStrokeTexture));

    public Texture DrawingStrokeTexture
    {
        get => GetValue(DrawingStrokeTextureProperty);
        set => SetValue(DrawingStrokeTextureProperty, value);
    }

    public Brush Brush
    {
        get { return (Brush)GetValue(BrushProperty); }
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
            x.StopStrokePreviewLoop();
            x.DrawingStrokeTexture = x.Brush?.StrokePreview;
        });
    }

    public BrushItem()
    {
        InitializeComponent();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        StartStrokePreviewLoop();
        isPreviewingStroke = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        StopStrokePreviewLoop();
    }

    private void StartStrokePreviewLoop()
    {
        if (isPreviewingStroke)
            return;

        BrushOutputNode? brushNode =
            Brush.Document?.AccessInternalReadOnlyDocument().NodeGraph.LookupNode(Brush.Id) as BrushOutputNode;
        if (brushNode == null)
            return;

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
        previewImage.CancelChanges();
        enumerator = brushNode.DrawStrokePreviewEnumerable(previewImage, CreateContext(),
            BrushOutputNode.StrokePreviewSizeY / 2,
            new VecD(0, BrushOutputNode.YOffsetInPreview)).GetEnumerator();

        previewTexture.DrawingSurface.Canvas.Clear();
        previewTimer = DispatcherTimer.Run(() =>
        {
            if (!enumerator.MoveNext())
            {
                isPreviewingStroke = false;
                DrawingStrokeTexture = Brush?.StrokePreview;
                return false;
            }

            previewImage.DrawMostUpToDateRegionOn(
                new RectI(0, 0, previewImage.CommittedSize.X, previewImage.CommittedSize.Y),
                ChunkResolution.Full,
                previewTexture.DrawingSurface.Canvas,
                VecI.Zero, null, SamplingOptions.Bilinear);

            StrokePreviewControl.QueueNextFrame();

            return isPreviewingStroke;
        }, TimeSpan.FromMilliseconds(8));
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
            Brush.Document.AccessInternalReadOnlyDocument().NodeGraph);
    }

    private void StopStrokePreviewLoop()
    {
        isPreviewingStroke = false;
        previewTimer?.Dispose();
        if (enumerator is IDisposable disposable)
        {
            disposable.Dispose();
        }

        DrawingStrokeTexture = Brush?.StrokePreview;
    }
}
