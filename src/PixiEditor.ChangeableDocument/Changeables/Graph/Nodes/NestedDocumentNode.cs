using Drawie.Backend.Core.Surfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("NestedDocument")]
public class NestedDocumentNode : RenderNode
{
    public InputProperty<IReadOnlyDocument> NestedDocument { get; }

    public NestedDocumentNode()
    {
        NestedDocument = CreateInput<IReadOnlyDocument>("Document", "DOCUMENT", null)
            .NonOverridenChanged(DocumentChanged);
    }

    private void DocumentChanged(IReadOnlyDocument document)
    {
        ClearOutputProperties();

        if (document is null)
            return;

        var brushOutput = document.NodeGraph.AllNodes.OfType<BrushOutputNode>().FirstOrDefault();

        if (brushOutput is null)
            return;

        foreach (var input in brushOutput.InputProperties)
        {
            if (input.InternalPropertyName == Output.InternalPropertyName)
                continue;

            AddOutputProperty(new OutputProperty(this, input.InternalPropertyName, input.DisplayName, input.Value,
                input.ValueType));
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

        var clonedContext = context.Clone();
        clonedContext.Graph = NestedDocument.Value.NodeGraph;
        clonedContext.DocumentSize = NestedDocument.Value.Size;
        clonedContext.ProcessingColorSpace = NestedDocument.Value.ProcessingColorSpace;
        clonedContext.VisibleDocumentRegion = null;

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

        base.OnExecute(context);
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
    }

    public override Node CreateCopy()
    {
        return new NestedDocumentNode();
    }
}
