using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Ellipse", "ELLIPSE_NODE")]
public class EllipseNode : Node
{
    public InputProperty<VecI> Radius { get; }
    public InputProperty<Color> StrokeColor { get; }
    public InputProperty<Color> FillColor { get; }
    public InputProperty<int> StrokeWidth { get; }
    public OutputProperty<Texture> Output { get; }

    private ChunkyImage? workingImage;
    private Texture? targetSurface;

    private VecI _lastRadius = new VecI(-1, -1);
    private Color _lastStrokeColor = new Color(0, 0, 0, 0);
    private Color _lastFillColor = new Color(0, 0, 0, 0);
    private int _lastStrokeWidth = -1;
    private Paint replacingPaint = new Paint() { BlendMode = BlendMode.Src };

    public EllipseNode()
    {
        Radius = CreateInput<VecI>("Radius", "RADIUS", new VecI(32, 32)).WithRules(
            v => v.Min(VecI.One));
        StrokeColor = CreateInput<Color>("StrokeColor", "STROKE_COLOR", new Color(0, 0, 0, 255));
        FillColor = CreateInput<Color>("FillColor", "FILL_COLOR", new Color(0, 0, 0, 255));
        StrokeWidth = CreateInput<int>("StrokeWidth", "STROKE_WIDTH", 1);
        Output = CreateOutput<Texture?>("Output", "OUTPUT", null);
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        var radius = Radius.Value;
        VecI targetDimensions = radius * 2;

        if (workingImage is null || workingImage.LatestSize.X != targetDimensions.X ||
            workingImage.LatestSize.Y != targetDimensions.Y)
        {
            if (targetDimensions.X <= 0 || targetDimensions.Y <= 0)
            {
                Output.Value = null;
                return null;
            }

            workingImage?.Dispose();
            workingImage = new ChunkyImage(targetDimensions);

            targetSurface = RequestTexture(0, targetDimensions);
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

        workingImage.DrawMostUpToDateChunkOn(context.ChunkToUpdate, context.ChunkResolution,
            targetSurface.DrawingSurface, VecI.Zero,
            replacingPaint);

        Output.Value = targetSurface;
        return targetSurface;
    }

    public override Node CreateCopy() => new EllipseNode();
}
