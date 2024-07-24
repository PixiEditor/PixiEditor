using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Ellipse")]
public class EllipseNode : Node
{
    public InputProperty<VecI> Radius { get; }
    public InputProperty<Color> StrokeColor { get; }
    public InputProperty<Color> FillColor { get; }
    public InputProperty<int> StrokeWidth { get; }
    public OutputProperty<Surface> Output { get; }

    private ChunkyImage? workingImage;
    private Surface? targetSurface;

    private VecI _lastRadius = new VecI(-1, -1);
    private Color _lastStrokeColor = new Color(0, 0, 0, 0);
    private Color _lastFillColor = new Color(0, 0, 0, 0);
    private int _lastStrokeWidth = -1;
    private Paint replacingPaint = new Paint() { BlendMode = BlendMode.Src };

    public EllipseNode()
    {
        Radius = CreateInput<VecI>("Radius", "RADIUS", new VecI(32, 32));
        StrokeColor = CreateInput<Color>("StrokeColor", "STROKE_COLOR", new Color(0, 0, 0, 255));
        FillColor = CreateInput<Color>("FillColor", "FILL_COLOR", new Color(0, 0, 0, 255));
        StrokeWidth = CreateInput<int>("StrokeWidth", "STROKE_WIDTH", 1);
        Output = CreateOutput<Surface?>("Output", "OUTPUT", null);
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        var radius = Radius.Value;
        VecI targetDimensions = radius * 2;

        if (workingImage is null || workingImage.LatestSize.X != targetDimensions.X ||
            workingImage.LatestSize.Y != targetDimensions.Y)
        {
            workingImage?.Dispose();

            if (Radius.Value.LongestAxis <= 0)
            {
                Output.Value = null;
                return null;
            }
            
            workingImage = new ChunkyImage(targetDimensions);

            targetSurface = new Surface(targetDimensions);
        }

        if (radius != _lastRadius || StrokeColor.Value != _lastStrokeColor || FillColor.Value != _lastFillColor ||
            StrokeWidth.Value != _lastStrokeWidth)
        {
            _lastRadius = radius;
            _lastStrokeColor = StrokeColor.Value;
            _lastFillColor = FillColor.Value;
            _lastStrokeWidth = StrokeWidth.Value;

            RectI location = new RectI(VecI.Zero, targetDimensions);
            workingImage.EnqueueDrawEllipse(location, StrokeColor.Value, FillColor.Value, StrokeWidth.Value);
            workingImage.CommitChanges();
        }

        workingImage.DrawMostUpToDateChunkOn(context.ChunkToUpdate, context.ChunkResolution, targetSurface.DrawingSurface, VecI.Zero,
            replacingPaint);

        Output.Value = targetSurface;
        return targetSurface;
    }

    public override string DisplayName { get; set; } = "ELLIPSE_NODE";

    public override Node CreateCopy() => new EllipseNode();
}
