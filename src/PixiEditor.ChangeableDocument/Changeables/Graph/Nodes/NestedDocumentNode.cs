using System.Drawing;
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

[NodeInfo(NodeId)]
public class NestedDocumentNode : LayerNode, IInputDependentOutputs, ITransformableObject, IRasterizable,
    IVariableSampling
{
    public const int MaxRecursionDepth = 5;
    public const string DocumentPropertyName = "Document";
    public const string NodeId = "NestedDocument";
    private DocumentReference? lastDocument;
    public InputProperty<DocumentReference> NestedDocument { get; }

    public InputProperty<bool> BilinearSampling { get; }

    public InputProperty<bool> ClipToDocumentBounds { get; }

    public OutputProperty<IReadOnlyNodeGraph> Graph { get; }

    public Matrix3X3 TransformationMatrix { get; set; } = Matrix3X3.Identity;


    public RectD TransformedAABB => new ShapeCorners(NestedDocument.Value?.DocumentInstance?.Size / 2f ?? VecD.Zero,
            NestedDocument.Value?.DocumentInstance?.Size ?? VecD.Zero)
        .WithMatrix(TransformationMatrix).AABBBounds;

    private IReadOnlyDocument? Instance => NestedDocument.Value?.DocumentInstance;

    private string[] builtInOutputs;
    private string[] builtInInputs;

    private ExposeValueNode[]? cachedExposeNodes;
    private BrushOutputNode[]? brushOutputNodes;
    private IReadOnlyNode[] toExecute;

    protected override bool MustRenderInSrgb(SceneObjectRenderContext ctx) => false;

    public NestedDocumentNode()
    {
        NestedDocument = CreateInput<DocumentReference>(DocumentPropertyName, "DOCUMENT", null)
            .NonOverridenChanged(DocumentChanged);
        NestedDocument.ConnectionChanged += NestedDocumentOnConnectionChanged;
        BilinearSampling = CreateInput<bool>("BilinearSampling", "BILINEAR_SAMPLING", false);
        Graph = CreateOutput<IReadOnlyNodeGraph>("Graph", "GRAPH", null);
        ClipToDocumentBounds = CreateInput<bool>("ClipToDocumentBounds", "CLIP_TO_BOUNDS", true);
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
        lastDocument = NestedDocument.Value;
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

        toExecute = cachedExposeNodes.Concat<IReadOnlyNode>(brushOutputNodes).Concat([Instance?.NodeGraph.OutputNode])
            .ToArray();

        Instance?.NodeGraph.Execute(cachedExposeNodes.Concat<IReadOnlyNode>(brushOutputNodes), new RenderContext(null,
            0,
            ChunkResolution.Full,
            Instance.Size, Instance.Size,
            Instance.ProcessingColorSpace,
            SamplingOptions.Default,
            Instance.NodeGraph) { FullRerender = true });

        foreach (var input in cachedExposeNodes)
        {
            if (input.Name.Value == Output.InternalPropertyName)
                continue;

            var firstExisting = OutputProperties.FirstOrDefault(x =>
                x.InternalPropertyName == input.Name.Value);
            if (firstExisting != null)
            {
                firstExisting.Value = input.Value.Value;
                continue;
            }

            AddOutputProperty(new OutputProperty(this, input.Name.Value, input.Name.Value, input.Value.Value,
                input.Value.Value?.GetType() ?? typeof(object)));
        }

        foreach (var brushOutput in brushOutputNodes)
        {
            if (OutputProperties.Any(x =>
                    brushOutput.InputProperties.Any(prop =>
                        $"{brushOutput.BrushName}_{prop.InternalPropertyName}" == x.InternalPropertyName)))
                continue;

            foreach (var output in brushOutput.InputProperties)
            {
                AddOutputProperty(new OutputProperty(this, $"{brushOutput.BrushName}_{output.InternalPropertyName}",
                    output.DisplayName,
                    output.Value, output.ValueType));
            }
        }

        foreach (var variable in document.DocumentInstance.NodeGraph.Blackboard.Variables)
        {
            if (InputProperties.Any(x =>
                    x.InternalPropertyName == variable.Key && x.ValueType == variable.Value.Type))
            {
                continue;
            }

            if (!variable.Value.IsExposed)
                continue;

            var existing = InputProperties.FirstOrDefault(x =>
                x.InternalPropertyName == variable.Key);
            if (existing != null) // Existing input with same name but different type
            {
                RemoveInputProperty(existing);
            }

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
                                    .All(prop =>
                                        $"{brushOutput.BrushName}_{prop.InternalPropertyName}" !=
                                        output.InternalPropertyName));

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
                                              x.Value.Type != input.ValueType) ||
                                !document.DocumentInstance.NodeGraph.Blackboard.Variables[input.InternalPropertyName]
                                    .IsExposed;

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
            DocumentChanged(NestedDocument.Value);
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

        var clonedContext = context.Clone();
        if (clonedContext.CloneDepth >= MaxRecursionDepth)
        {
            return;
        }

        clonedContext.Graph = Instance?.NodeGraph;
        clonedContext.DocumentSize = Instance.Size;
        clonedContext.ProcessingColorSpace = Instance?.ProcessingColorSpace;
        clonedContext.VisibleDocumentRegion = null;
        clonedContext.RenderSurface = null;

        Instance?.NodeGraph.Execute(toExecute, clonedContext);

        if (AnyConnectionExists())
        {
            foreach (var output in OutputProperties)
            {
                if (output.InternalPropertyName == Output.InternalPropertyName)
                    continue;

                var correspondingExposeNode = cachedExposeNodes?
                    .FirstOrDefault(x => x.Name.Value == output.InternalPropertyName);

                if (correspondingExposeNode is null)
                {
                    var correspondingBrushNode = brushOutputNodes?
                        .FirstOrDefault(brushOutput => brushOutput.InputProperties
                            .Any(prop =>
                                $"{brushOutput.BrushName}_{prop.InternalPropertyName}" == output.InternalPropertyName &&
                                prop.ValueType == output.ValueType));
                    if (correspondingBrushNode is not null)
                    {
                        var correspondingProp = correspondingBrushNode.InputProperties
                            .First(prop =>
                                $"{correspondingBrushNode.BrushName}_{prop.InternalPropertyName}" ==
                                output.InternalPropertyName &&
                                prop.ValueType == output.ValueType);
                        output.Value = correspondingProp.Value;
                    }

                    continue;
                }

                output.Value = correspondingExposeNode.Value.Value;
            }
        }

        Graph.Value = Instance.NodeGraph;
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, Canvas workingSurface, Paint paint)
    {
        RenderNested(ctx, workingSurface, paint);
    }

    protected override void DrawWithFilters(SceneObjectRenderContext ctx, Canvas workingSurface, Paint paint)
    {
        RenderNested(ctx, workingSurface, paint);
    }

    public void Rasterize(Canvas surface, Paint paint, int atFrame)
    {
        RenderContext context = new(
            surface, atFrame, ChunkResolution.Full,
            surface.DeviceClipBounds.Size,
            Instance.Size,
            Instance.ProcessingColorSpace,
            BilinearSampling.Value ? SamplingOptions.Bilinear : SamplingOptions.Default,
            Instance.NodeGraph) { FullRerender = true, };

        RenderNested(context, surface, paint);
    }

    private void RenderNested(RenderContext ctx, Canvas workingSurface, Paint paint)
    {
        if (NestedDocument.Value is null)
            return;

        using var intermediate = Texture.ForProcessing(workingSurface.Surface, Instance.ProcessingColorSpace);
        int workingSurfaceSaved = 0;
        if (paint.IsOpaqueStandardNonBlendingPaint)
        {
            workingSurfaceSaved = workingSurface.Save();
        }
        else
        {
            workingSurfaceSaved = workingSurface.SaveLayer(paint);
        }

        workingSurface.SetMatrix(Matrix3X3.Identity);

        Canvas targetSurface = intermediate.DrawingSurface.Canvas;

        targetSurface.SetMatrix(targetSurface.TotalMatrix.Concat(TransformationMatrix));
        if (ClipToDocumentBounds.Value)
        {
            var docSize = NestedDocument.Value.DocumentInstance.Size;
            targetSurface.ClipRect(new RectD(VecI.Zero, docSize));
        }

        var clonedCtx = ctx.Clone();
        clonedCtx.RenderSurface = targetSurface;
        ExecuteNested(clonedCtx);

        Paint? paintToApply = null;
        if (!Instance.ProcessingColorSpace.IsSrgb && paint.ColorFilter == null)
        {
            // This is a weird hack to make alpha between linear -> srgb not glitch if the nested document does something weird with alpha.
            // Look at NestedColorSpaceOverlayAlpha.pixi in RenderTests and see what happens when you remove this line.
            paintToApply = new Paint();
            paintToApply.ColorFilter = ColorFilter.CreateColorMatrix(ColorMatrix.Identity);
        }

        workingSurface.DrawSurface(intermediate.DrawingSurface, 0, 0, paintToApply);
        workingSurface.RestoreToCount(workingSurfaceSaved);

        paintToApply?.ColorFilter?.Dispose();
        paintToApply?.Dispose();
    }


    private void ExecuteNested(RenderContext ctx)
    {
        var clonedContext = ctx.Clone();
        if (clonedContext.CloneDepth >= MaxRecursionDepth)
        {
            return;
        }

        clonedContext.Graph = Instance?.NodeGraph;
        clonedContext.DocumentSize = Instance?.Size ?? VecI.Zero;
        clonedContext.ProcessingColorSpace = Instance?.ProcessingColorSpace;
        clonedContext.RenderOutputSize =
            (VecI)(clonedContext.DocumentSize * clonedContext.ChunkResolution.Multiplier());
        clonedContext.DesiredSamplingOptions =
            BilinearSampling.Value ? SamplingOptions.Bilinear : SamplingOptions.Default;
        if (clonedContext.VisibleDocumentRegion.HasValue)
        {
            var inverted =
                new ShapeCorners((RectD)clonedContext.VisibleDocumentRegion.Value).WithMatrix(TransformationMatrix
                    .Invert());
            RectD docRegion = new RectD(VecI.Zero, Instance?.Size ?? VecI.Zero);
            RectD intersection = docRegion.Intersect(inverted.AABBBounds);
            clonedContext.VisibleDocumentRegion = (RectI)intersection.RoundOutwards();
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
        if (renderOn is null) return;

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

    internal override void SerializeAdditionalDataInternal(IReadOnlyDocument target,
        Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalDataInternal(target, additionalData);
        additionalData["lastDocument"] = lastDocument;
        additionalData["TransformationMatrix"] = TransformationMatrix;
    }

    internal override void DeserializeAdditionalDataInternal(IReadOnlyDocument target,
        IReadOnlyDictionary<string, object> data,
        List<IChangeInfo> infos)
    {
        base.DeserializeAdditionalDataInternal(target, data, infos);
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

    public override void Dispose()
    {
        Graph.Value = null; // Prevent disposing nested document's graph
        base.Dispose();
    }

    private bool AnyConnectionExists()
    {
        foreach (var output in OutputProperties)
        {
            if (output.Connections.Count > 0)
                return true;
        }

        return false;
    }
}
