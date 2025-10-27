using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("NestedDocument")]
public class NestedDocumentNode : LayerNode, IInputDependentOutputs, ITransformableObject, IRasterizable,
    IVariableSampling
{
    private DocumentReference? lastDocument;
    public InputProperty<DocumentReference> NestedDocument { get; }

    public InputProperty<bool> BilinearSampling { get; }

    public OutputProperty<IReadOnlyNodeGraph> Graph { get; }

    public Matrix3X3 TransformationMatrix { get; set; } = Matrix3X3.Identity;

    public RectD TransformedAABB => new ShapeCorners(NestedDocument.Value?.DocumentInstance?.Size / 2f ?? VecD.Zero,
            NestedDocument.Value?.DocumentInstance?.Size ?? VecD.Zero)
        .WithMatrix(TransformationMatrix).AABBBounds;

    private IReadOnlyDocument? Instance => NestedDocument.Value?.DocumentInstance;

    private Texture? dummyTexture;

    private string[] builtInOutputs;
    private string[] builtInInputs;

    private ExposeValueNode[]? cachedExposeNodes;
    private BrushOutputNode[]? brushOutputNodes;

    public NestedDocumentNode()
    {
        NestedDocument = CreateInput<DocumentReference>("Document", "DOCUMENT", null)
            .NonOverridenChanged(DocumentChanged);
        NestedDocument.ConnectionChanged += NestedDocumentOnConnectionChanged;
        BilinearSampling = CreateInput<bool>("BilinearSampling", "BILINEAR_SAMPLING", false);
        Graph = CreateOutput<IReadOnlyNodeGraph>("Graph", "GRAPH", null);
        AllowHighDpiRendering = true;

        builtInOutputs = OutputProperties.Select(x => x.InternalPropertyName).ToArray();
        builtInInputs = InputProperties.Select(x => x.InternalPropertyName).ToArray();
    }

    protected override int GetContentCacheHash()
    {
        return HashCode.Combine(base.GetContentCacheHash(), TransformationMatrix);
    }

    private void NestedDocumentOnConnectionChanged()
    {
        if (NestedDocument.Value == null && NestedDocument.Connection != null) return;

        DocumentChanged(NestedDocument.Value);
    }

    private void DocumentChanged(DocumentReference document)
    {
        if (document?.DocumentInstance == null)
        {
            ClearOutputProperties();
            ClearInputProperties();
            cachedExposeNodes = null;
            return;
        }
        
        cachedExposeNodes = document.DocumentInstance.NodeGraph.AllNodes
            .OfType<ExposeValueNode>().ToArray();
        
        brushOutputNodes = document.DocumentInstance.NodeGraph.AllNodes
            .OfType<BrushOutputNode>().ToArray();
        
        Instance?.NodeGraph.Execute(cachedExposeNodes.Concat<IReadOnlyNode>(brushOutputNodes), new RenderContext(
            (dummyTexture ??= Texture.ForProcessing(VecI.One, Instance.ProcessingColorSpace)).DrawingSurface.Canvas, 0, ChunkResolution.Full,
            Instance.Size, Instance.Size,
            Instance.ProcessingColorSpace,
            SamplingOptions.Default,
            Instance.NodeGraph) { FullRerender = true });

        foreach (var input in cachedExposeNodes)
        {
            if (input.Name.Value == Output.InternalPropertyName)
                continue;

            if (OutputProperties.Any(x =>
                    x.InternalPropertyName == input.Name.Value))
                continue;

            AddOutputProperty(new OutputProperty(this, input.Name.Value, input.Name.Value, input.Value.Value,
                input.Value.Value?.GetType() ?? typeof(object)));
        }
        
        foreach (var brushOutput in brushOutputNodes)
        {
            if (OutputProperties.Any(x =>
                    brushOutput.InputProperties.Any(prop => $"{brushOutput.BrushName}_{prop.InternalPropertyName}" == x.InternalPropertyName)))
                continue;

            foreach (var output in brushOutput.InputProperties)
            {
                AddOutputProperty(new OutputProperty(this, $"{brushOutput.BrushName}_{output.InternalPropertyName}", output.DisplayName,
                    output.Value, output.ValueType));
            }
        }

        foreach (var variable in document.DocumentInstance.NodeGraph.Blackboard.Variables)
        {
            if (InputProperties.Any(x =>
                    x.InternalPropertyName == variable.Key && x.ValueType == variable.Value.Type))
                continue;

            AddInputProperty(new InputProperty(this, variable.Key, variable.Key, variable.Value.Value,
                variable.Value.Type));
        }

        for (int i = OutputProperties.Count - 1; i >= 0; i--)
        {
            var output = OutputProperties[i];
            if (builtInOutputs.Contains(output.InternalPropertyName))
                continue;

            bool shouldRemove = cachedExposeNodes.All(x => x.Name.Value != output.InternalPropertyName) &&
                                brushOutputNodes.All(brushOutput => brushOutput.InputProperties
                                    .All(prop => $"{brushOutput.BrushName}_{prop.InternalPropertyName}" != output.InternalPropertyName));
            
            if (shouldRemove)
            {
                RemoveOutputProperty(output);
            }
        }
        
        for (int i = InputProperties.Count - 1; i >= 0; i--)
        {
            var input = InputProperties[i];
            if (builtInInputs.Contains(input.InternalPropertyName))
                continue;

            bool shouldRemove = document.DocumentInstance.NodeGraph.Blackboard.Variables
                .All(x => x.Key != input.InternalPropertyName ||
                          x.Value.Type != input.ValueType);
            
            if (shouldRemove)
            {
                RemoveInputProperty(input);
            }
        }
    }

    private void ClearOutputProperties()
    {
        var toRemove = OutputProperties
            .Where(x => !builtInOutputs.Contains(x.InternalPropertyName))
            .ToList();
        foreach (var property in toRemove)
        {
            RemoveOutputProperty(property);
        }
    }
    
    private void ClearInputProperties()
    {
        var toRemove = InputProperties
            .Where(x => !builtInInputs.Contains(x.InternalPropertyName))
            .ToList();
        foreach (var property in toRemove)
        {
            RemoveInputProperty(property);
        }
    }

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);

        if (Instance is null)
            return;

        if (Instance != lastDocument?.DocumentInstance)
        {
            lastDocument = NestedDocument.Value;
            DocumentChanged(NestedDocument.Value);
        }

        if (AnyConnectionExists())
        {
            var clonedContext = context.Clone();
            clonedContext.Graph = Instance?.NodeGraph;
            clonedContext.DocumentSize = Instance.Size;
            clonedContext.ProcessingColorSpace = Instance?.ProcessingColorSpace;
            clonedContext.VisibleDocumentRegion = null;
            clonedContext.RenderSurface =
                (dummyTexture ??= Texture.ForProcessing(new VecI(1, 1), context.ProcessingColorSpace)).DrawingSurface
                .Canvas;

            Instance?.NodeGraph.Execute(cachedExposeNodes.Concat<IReadOnlyNode>(brushOutputNodes), clonedContext);

            foreach (var output in OutputProperties)
            {
                if (output.InternalPropertyName == Output.InternalPropertyName)
                    continue;

                var correspondingExposeNode = cachedExposeNodes?
                    .FirstOrDefault(x => x.Name.Value == output.InternalPropertyName &&
                                         x.Value.ValueType == output.ValueType);

                if (correspondingExposeNode is null)
                {
                    var correspondingBrushNode = brushOutputNodes?
                        .FirstOrDefault(brushOutput => brushOutput.InputProperties
                            .Any(prop => $"{brushOutput.BrushName}_{prop.InternalPropertyName}" == output.InternalPropertyName &&
                                         prop.ValueType == output.ValueType));
                    if (correspondingBrushNode is not null)
                    {
                        var correspondingProp = correspondingBrushNode.InputProperties
                            .First(prop => $"{correspondingBrushNode.BrushName}_{prop.InternalPropertyName}" == output.InternalPropertyName &&
                                           prop.ValueType == output.ValueType);
                        output.Value = correspondingProp.Value;
                    }
                    
                    continue;
                }

                output.Value = correspondingExposeNode.Value.Value;
            }
        }

        foreach (var blackboardVariable in Instance?.NodeGraph.Blackboard.Variables)
        {
            var input = InputProperties.FirstOrDefault(x =>
                x.InternalPropertyName == blackboardVariable.Key &&
                x.ValueType == blackboardVariable.Value.Type);

            if (input is null || blackboardVariable.Value is not Variable variable)
                continue;

            variable.Value = input.Value;
        }

        Graph.Value = Instance.NodeGraph;
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, Canvas workingSurface, Paint paint)
    {
        if (NestedDocument.Value is null)
            return;

        int saved;
        if (paint.IsOpaqueStandardNonBlendingPaint)
        {
            saved = workingSurface.Save();
        }
        else
        {
            saved = workingSurface.SaveLayer(paint);
        }

        workingSurface.SetMatrix(workingSurface.TotalMatrix.Concat(TransformationMatrix));

        ExecuteNested(ctx);

        workingSurface.RestoreToCount(saved);
    }


    protected override void DrawWithFilters(SceneObjectRenderContext ctx, Canvas workingSurface, Paint paint)
    {
        if (NestedDocument.Value is null)
            return;

        int saved = workingSurface.SaveLayer(paint);

        workingSurface.SetMatrix(workingSurface.TotalMatrix.Concat(TransformationMatrix));

        ExecuteNested(ctx);

        workingSurface.RestoreToCount(saved);
    }

    public void Rasterize(Canvas surface, Paint paint, int atFrame)
    {
        if (NestedDocument.Value is null)
            return;

        int layer;
        if (paint is { IsOpaqueStandardNonBlendingPaint: false })
        {
            layer = surface.SaveLayer(paint);
        }
        else
        {
            layer = surface.Save();
        }

        surface.SetMatrix(surface.TotalMatrix.Concat(TransformationMatrix));

        RenderContext context = new(
            surface, atFrame, ChunkResolution.Full,
            surface.DeviceClipBounds.Size,
            Instance.Size,
            Instance.ProcessingColorSpace,
            BilinearSampling.Value ? SamplingOptions.Bilinear : SamplingOptions.Default,
            Instance.NodeGraph) { FullRerender = true, };

        ExecuteNested(context);

        surface.RestoreToCount(layer);
    }

    private void ExecuteNested(RenderContext ctx)
    {
        var clonedContext = ctx.Clone();
        clonedContext.Graph = Instance?.NodeGraph;
        clonedContext.DocumentSize = Instance?.Size ?? VecI.Zero;
        clonedContext.ProcessingColorSpace = Instance?.ProcessingColorSpace;
        clonedContext.DesiredSamplingOptions =
            BilinearSampling.Value ? SamplingOptions.Bilinear : SamplingOptions.Default;
        if (clonedContext.VisibleDocumentRegion.HasValue)
        {
            clonedContext.VisibleDocumentRegion =
                (RectI)new ShapeCorners((RectD)clonedContext.VisibleDocumentRegion.Value)
                    .WithMatrix(TransformationMatrix.Invert()).AABBBounds;
        }

        var outputNode = Instance?.NodeGraph.AllNodes.OfType<BrushOutputNode>().FirstOrDefault() ??
                         Instance?.NodeGraph.OutputNode;

        Instance?.NodeGraph.Execute(outputNode, clonedContext);
    }

    protected override bool ShouldRenderPreview(string elementToRenderName)
    {
        if (IsDisposed)
        {
            return false;
        }

        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.ShouldRenderPreview(elementToRenderName);
        }

        return NestedDocument.Value != null;
    }

    public override RectD? GetPreviewBounds(RenderContext ctx, string elementToRenderName)
    {
        return TransformedAABB;
    }

    public override void RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            base.RenderPreview(renderOn, context, elementToRenderName);
            return;
        }

        Paint(context, renderOn.Canvas);
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        return TransformedAABB;
    }

    public override RectD? GetApproxBounds(KeyFrameTime frameTime)
    {
        return TransformedAABB;
    }

    public override ShapeCorners GetTransformationCorners(KeyFrameTime frameTime)
    {
        return new ShapeCorners(Instance?.Size / 2f ?? VecD.Zero, Instance?.Size ?? VecD.Zero)
            .WithMatrix(TransformationMatrix);
    }

    public override void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalData(additionalData);
        additionalData["lastDocument"] = lastDocument;
        additionalData["TransformationMatrix"] = TransformationMatrix;
    }

    internal override void DeserializeAdditionalData(IReadOnlyDocument target, IReadOnlyDictionary<string, object> data,
        List<IChangeInfo> infos)
    {
        base.DeserializeAdditionalData(target, data, infos);
        if (data.TryGetValue("lastDocument", out var doc) && doc is DocumentReference document)
        {
            DocumentChanged(document); // restore outputs
            infos.Add(NodeOutputsChanged_ChangeInfo.FromNode(this));
        }

        if (data.TryGetValue("TransformationMatrix", out var matrix) && matrix is Matrix3X3 mat)
        {
            TransformationMatrix = mat;
        }
    }

    public override VecD GetScenePosition(KeyFrameTime frameTime)
    {
        return TransformedAABB.Center;
    }

    public override VecD GetSceneSize(KeyFrameTime frameTime)
    {
        return TransformedAABB.Size;
    }

    public void UpdateOutputs()
    {
        DocumentChanged(NestedDocument.Value);
    }

    public override Node CreateCopy()
    {
        return new NestedDocumentNode() { TransformationMatrix = this.TransformationMatrix };
    }

    private bool AnyConnectionExists()
    {
        foreach (var output in OutputProperties)
        {
            if (builtInOutputs.Contains(output.InternalPropertyName))
                continue;

            if (output.Connections.Count > 0)
                return true;
        }

        return false;
    }
}
