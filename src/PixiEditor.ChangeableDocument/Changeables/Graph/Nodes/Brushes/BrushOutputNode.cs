using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

[NodeInfo("BrushOutput")]
public class BrushOutputNode : Node
{
    public InputProperty<ShapeVectorData> VectorShape { get; }
    public InputProperty<Paintable> Stroke { get; }
    public InputProperty<Paintable> Fill { get; }
    public RenderInputProperty Content { get; }
    public InputProperty<float> Pressure { get; }
    public InputProperty<bool> FitToStrokeSize { get; }

    internal Texture ContentTexture;

    private TextureCache cache = new();

    protected override bool ExecuteOnlyOnCacheChange => true;

    public BrushOutputNode()
    {
        VectorShape = CreateInput<ShapeVectorData>("VectorShape", "SHAPE", null);
        Stroke = CreateInput<Paintable>("Stroke", "STROKE", null);
        Fill = CreateInput<Paintable>("Fill", "FILL", null);
        Content = CreateRenderInput("Content", "CONTENT");

        Pressure = CreateInput<float>("Pressure", "PRESSURE", 1f);
        FitToStrokeSize = CreateInput<bool>("FitToStrokeSize", "FIT_TO_STROKE_SIZE", true);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Content.Value != null)
        {
            ContentTexture = cache.RequestTexture(0, context.RenderOutputSize, context.ProcessingColorSpace);
            Content.Value.Paint(context, ContentTexture.DrawingSurface);
        }
    }

    public override Node CreateCopy()
    {
        return new BrushOutputNode();
    }
}
