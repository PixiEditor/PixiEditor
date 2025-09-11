using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("RepeatStart")]
[PairNode(typeof(RepeatNodeEnd), "RepeatZone", true)]
public class RepeatNodeStart : Node, IPairNode
{
    public InputProperty<int> Iterations { get; }
    public InputProperty<object> Input { get; }
    public OutputProperty<int> CurrentIteration { get; }
    public OutputProperty<object> Output { get; }

    public Guid OtherNode { get; set; }
    private RepeatNodeEnd? endNode;

    private bool iterationInProgress = false;

    public RepeatNodeStart()
    {
        Iterations = CreateInput<int>("Iterations", "ITERATIONS", 1);
        Input = CreateInput<object>("Input", "INPUT", null);
        CurrentIteration = CreateOutput<int>("CurrentIteration", "CURRENT_ITERATION", 0);
        Output = CreateOutput<object>("Output", "OUTPUT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (iterationInProgress)
        {
            return;
        }

        endNode = FindEndNode();
        if (endNode == null)
        {
            return;
        }

        OtherNode = endNode?.Id ?? Guid.Empty;

        int iterations = Iterations.Value;
        var queue = GraphUtils.CalculateExecutionQueue(endNode, true, true,
            property => property.Connection?.Node != this);

        Output.Value = Input.Value;
        iterationInProgress = true;
        for (int i = 0; i < iterations; i++)
        {
            CurrentIteration.Value = i + 1;
            foreach (var node in queue)
            {
                if (node == this)
                {
                    continue;
                }

                node.Execute(context);
            }


            Output.Value = endNode.Output.Value;
        }

        iterationInProgress = false;
    }

    private RepeatNodeEnd FindEndNode()
    {
        RepeatNodeEnd repeatNodeEnd = null;
        int nestingCount = 0;
        HashSet<Guid> visitedNodes = new HashSet<Guid>();
        TraverseForwards(node =>
        {
            if (node is RepeatNodeStart && node != this)
            {
                nestingCount++;
            }

            if (node is RepeatNodeEnd rightNode && nestingCount == 0 && rightNode.OtherNode == Id)
            {
                repeatNodeEnd = rightNode;
                return false;
            }

            if (node is RepeatNodeEnd && visitedNodes.Add(node.Id))
            {
                nestingCount--;
            }

            return true;
        });

        return repeatNodeEnd;
    }

    public override Node CreateCopy()
    {
        return new RepeatNodeStart();
    }
}
