using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

[NodeInfo(NodeId)]
public class BrushOutputNode : Node
{
    public const string NodeId = "BrushOutput";
    public const string BrushNameProperty = "BrushName";

    public InputProperty<string> BrushName { get; }
    public InputProperty<ShapeVectorData> VectorShape { get; }
    public InputProperty<Paintable> Stroke { get; }
    public InputProperty<Paintable> Fill { get; }
    public RenderInputProperty Content { get; }
    public InputProperty<BlendMode> BlendMode { get; }
    public InputProperty<Matrix3X3> Transform { get; }
    public InputProperty<float> Pressure { get; }
    public InputProperty<bool> FitToStrokeSize { get; }
    public InputProperty<bool> AutoPosition { get; }
    public InputProperty<bool> AllowSampleStacking { get; }

    internal Texture ContentTexture;

    private TextureCache cache = new();

    protected override bool ExecuteOnlyOnCacheChange => true;

    public BrushOutputNode()
    {
        BrushName = CreateInput<string>(BrushNameProperty, "NAME", "Unnamed");
        VectorShape = CreateInput<ShapeVectorData>("VectorShape", "SHAPE", null);
        Stroke = CreateInput<Paintable>("Stroke", "STROKE", null);
        Fill = CreateInput<Paintable>("Fill", "FILL", null);
        Content = CreateRenderInput("Content", "CONTENT");
        Transform = CreateInput<Matrix3X3>("Transform", "TRANSFORM", Matrix3X3.Identity);
        BlendMode = CreateInput<BlendMode>("BlendMode", "BLEND_MODE", Enums.BlendMode.Normal);

        Pressure = CreateInput<float>("Pressure", "PRESSURE", 1f);
        FitToStrokeSize = CreateInput<bool>("FitToStrokeSize", "FIT_TO_STROKE_SIZE", true);
        AutoPosition = CreateInput<bool>("AutoPosition", "AUTO_POSITION", true);
        AllowSampleStacking = CreateInput<bool>("AllowSampleStacking", "ALLOW_SAMPLE_STACKING", false);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Content.Value != null)
        {
            ContentTexture = cache.RequestTexture(0, context.RenderOutputSize, context.ProcessingColorSpace);
            ContentTexture.DrawingSurface.Canvas.Save();
            ContentTexture.DrawingSurface.Canvas.SetMatrix(Transform.Value);
            Content.Value.Paint(context, ContentTexture.DrawingSurface);
            ContentTexture.DrawingSurface.Canvas.Restore();
        }
    }

    public override Node CreateCopy()
    {
        return new BrushOutputNode();
    }
}
