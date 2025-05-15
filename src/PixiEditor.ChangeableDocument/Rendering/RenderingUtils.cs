using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Rendering;

public static class RenderingUtils
{
    public static IReadOnlyNodeGraph SolveFinalNodeGraph(string? targetOutput, IReadOnlyDocument document)
    {
        if (targetOutput is null or "DEFAULT")
        {
            return document.NodeGraph;
        }

        CustomOutputNode[] outputNodes = document.NodeGraph.AllNodes.OfType<CustomOutputNode>().ToArray();

        foreach (CustomOutputNode outputNode in outputNodes)
        {
            if (outputNode.OutputName.Value == targetOutput)
            {
                return GraphFromOutputNode(outputNode);
            }
        }

        return document.NodeGraph;
    }

    public static IReadOnlyNodeGraph GraphFromOutputNode(CustomOutputNode outputNode)
    {
        NodeGraph graph = new();
        outputNode.TraverseBackwards(n =>
        {
            if (n is Node node)
            {
                graph.AddNode(node);
            }

            return true;
        });

        graph.CustomOutputNode = outputNode;
        return graph;
    }
}
