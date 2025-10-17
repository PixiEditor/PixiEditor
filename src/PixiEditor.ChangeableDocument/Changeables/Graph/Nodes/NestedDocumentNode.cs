using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("NestedDocument")]
public class NestedDocumentNode : LayerNode, IInputDependentOutputs, ITransformableObject
{
    private IReadOnlyDocument? lastDocument;
    public InputProperty<IReadOnlyDocument> NestedDocument { get; }

    public Matrix3X3 TransformationMatrix { get; set; } = Matrix3X3.Identity;

    public RectD TransformedAABB => new ShapeCorners(NestedDocument.Value?.Size / 2f ?? VecD.Zero, NestedDocument.Value?.Size ?? VecD.Zero)
        .WithMatrix(TransformationMatrix).AABBBounds;

    private Texture? dummyTexture;

    public NestedDocumentNode()
    {
        NestedDocument = CreateInput<IReadOnlyDocument>("Document", "DOCUMENT", null)
            .NonOverridenChanged(DocumentChanged);
        NestedDocument.ConnectionChanged += NestedDocumentOnConnectionChanged;
        AllowHighDpiRendering = true;
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

    private void DocumentChanged(IReadOnlyDocument document)
    {
        if (document == null)
        {
            ClearOutputProperties();
            return;
        }

        var brushOutput = document.NodeGraph.AllNodes.OfType<BrushOutputNode>().FirstOrDefault();

        if (brushOutput is null)
            return;

        foreach (var input in brushOutput.InputProperties)
        {
            if (input.InternalPropertyName == Output.InternalPropertyName)
                continue;

            if (OutputProperties.Any(x =>
                    x.InternalPropertyName == input.InternalPropertyName && x.ValueType == input.ValueType))
                continue;

            AddOutputProperty(new OutputProperty(this, input.InternalPropertyName, input.DisplayName, input.Value,
                input.ValueType));
        }

        for (int i = OutputProperties.Count - 1; i >= 0; i--)
        {
            var output = OutputProperties[i];
            if (output.InternalPropertyName == Output.InternalPropertyName)
                continue;

            var correspondingInput = brushOutput.InputProperties.FirstOrDefault(x =>
                x.InternalPropertyName == output.InternalPropertyName && x.ValueType == output.ValueType);

            if (correspondingInput is null)
            {
                RemoveOutputProperty(output);
            }
        }
    }

    private void ClearOutputProperties()
    {
        var toRemove = OutputProperties.Where(x => x.InternalPropertyName != Output.InternalPropertyName).ToList();
        foreach (var property in toRemove)
        {
            RemoveOutputProperty(property);
        }
    }

    protected override void OnExecute(RenderContext context)
    {
        if (NestedDocument.Value is null)
            return;

        if (NestedDocument.Value != lastDocument)
        {
            lastDocument = NestedDocument.Value;
            DocumentChanged(NestedDocument.Value);
        }

        if (AnyConnectionExists())
        {
            var clonedContext = context.Clone();
            clonedContext.Graph = NestedDocument.Value.NodeGraph;
            clonedContext.DocumentSize = NestedDocument.Value.Size;
            clonedContext.ProcessingColorSpace = NestedDocument.Value.ProcessingColorSpace;
            clonedContext.VisibleDocumentRegion = null;
            clonedContext.RenderSurface =
                (dummyTexture ??= Texture.ForProcessing(new VecI(1, 1), context.ProcessingColorSpace)).DrawingSurface;

            var outputNode = NestedDocument.Value.NodeGraph.AllNodes.OfType<BrushOutputNode>().FirstOrDefault() ??
                             NestedDocument.Value.NodeGraph.OutputNode;

            NestedDocument.Value?.NodeGraph.Execute(outputNode, clonedContext);

            foreach (var output in OutputProperties)
            {
                if (output.InternalPropertyName == Output.InternalPropertyName)
                    continue;

                var correspondingInput = outputNode.InputProperties.FirstOrDefault(x =>
                    x.InternalPropertyName == output.InternalPropertyName && x.ValueType == output.ValueType);

                if (correspondingInput is null)
                    continue;

                output.Value = correspondingInput.Value;
            }
        }

        base.OnExecute(context);
    }

    protected override void DrawWithoutFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface, Paint paint)
    {
        if (NestedDocument.Value is null)
            return;

        var clonedContext = ctx.Clone();
        clonedContext.Graph = NestedDocument.Value.NodeGraph;
        clonedContext.DocumentSize = NestedDocument.Value.Size;
        clonedContext.ProcessingColorSpace = NestedDocument.Value.ProcessingColorSpace;
        if (clonedContext.VisibleDocumentRegion.HasValue)
        {
            clonedContext.VisibleDocumentRegion =
                (RectI)new ShapeCorners((RectD)clonedContext.VisibleDocumentRegion.Value)
                    .WithMatrix(TransformationMatrix.Invert()).AABBBounds;
        }

        int saved = workingSurface.Canvas.Save();
        workingSurface.Canvas.SetMatrix(workingSurface.Canvas.TotalMatrix.Concat(TransformationMatrix));

        var outputNode = NestedDocument.Value.NodeGraph.AllNodes.OfType<BrushOutputNode>().FirstOrDefault() ??
                         NestedDocument.Value.NodeGraph.OutputNode;

        NestedDocument.Value?.NodeGraph.Execute(outputNode, clonedContext);

        workingSurface.Canvas.RestoreToCount(saved);
    }

    protected override void DrawWithFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface, Paint paint)
    {
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
        return new ShapeCorners(NestedDocument.Value?.Size / 2f ?? VecD.Zero, NestedDocument.Value?.Size ?? VecD.Zero)
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
        if (data.TryGetValue("lastDocument", out var doc) && doc is IReadOnlyDocument document)
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
            if (output.InternalPropertyName == Output.InternalPropertyName)
                continue;

            if (output.Connections.Count > 0)
                return true;
        }

        return false;
    }
}
