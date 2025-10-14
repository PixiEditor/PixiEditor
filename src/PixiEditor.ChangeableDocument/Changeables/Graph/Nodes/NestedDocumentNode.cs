using Drawie.Backend.Core.Surfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("NestedDocument")]
public class NestedDocumentNode : RenderNode, IInputDependentOutputs
{
    private IReadOnlyDocument? lastDocument;
    public InputProperty<IReadOnlyDocument> NestedDocument { get; }

    public NestedDocumentNode()
    {
        NestedDocument = CreateInput<IReadOnlyDocument>("Document", "DOCUMENT", null)
            .NonOverridenChanged(DocumentChanged);
        NestedDocument.ConnectionChanged += NestedDocumentOnConnectionChanged;
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

            if (OutputProperties.Any(x => x.InternalPropertyName == input.InternalPropertyName && x.ValueType == input.ValueType))
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

        if(NestedDocument.Value != lastDocument)
        {
            lastDocument = NestedDocument.Value;
            DocumentChanged(NestedDocument.Value);
        }

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

    public override void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
        additionalData["lastDocument"] = lastDocument;
    }

    internal override void DeserializeAdditionalData(IReadOnlyDocument target, IReadOnlyDictionary<string, object> data, List<IChangeInfo> infos)
    {
        if (data.TryGetValue("lastDocument", out var doc) && doc is IReadOnlyDocument document)
        {
            DocumentChanged(document); // restore outputs
            infos.Add(NodeOutputsChanged_ChangeInfo.FromNode(this));
        }
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
    }

    public override Node CreateCopy()
    {
        return new NestedDocumentNode();
    }

    public void UpdateOutputs()
    {
        DocumentChanged(NestedDocument.Value);
    }
}
