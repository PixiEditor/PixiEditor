using System.Collections.ObjectModel;
using Avalonia;
using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Nodes;

public class NodeViewModel : ObservableObject, INodeHandler
{
    private string nodeName;
    private VecD position;
    private ObservableCollection<IInputPropertyHandler> inputs = new();
    private ObservableCollection<IOutputPropertyHandler> outputs = new();
    private Surface resultPreview;

    protected Guid id;

    public NodeViewModel()
    {
        
    }

    public NodeViewModel(string nodeName, Guid id, VecD position)
    {
        this.nodeName = nodeName;
        this.id = id;
        this.position = position;
    }

    public Guid Id
    {
        get => id;
    }

    public string NodeName
    {
        get => nodeName;
        set => SetProperty(ref nodeName, value);
    }

    public VecD Position
    {
        get => position;
        set => SetProperty(ref position, value);
    }

    public ObservableCollection<IInputPropertyHandler> Inputs
    {
        get => inputs;
        set => SetProperty(ref inputs, value);
    }

    public ObservableCollection<IOutputPropertyHandler> Outputs
    {
        get => outputs;
        set => SetProperty(ref outputs, value);
    }

    public Surface ResultPreview
    {
        get => resultPreview;
        set => SetProperty(ref resultPreview, value);
    }

    public void TraverseBackwards(Func<INodeHandler, bool> func)
    {
        var visited = new HashSet<INodeHandler>();
        var queueNodes = new Queue<INodeHandler>();
        queueNodes.Enqueue(this);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add(node))
            {
                continue;
            }

            if (!func(node))
            {
                return;
            }

            foreach (var inputProperty in node.Inputs)
            {
                if (inputProperty.Connection != null)
                {
                    queueNodes.Enqueue(inputProperty.Node);
                }
            }
        }
    }

    public void TraverseForwards(Func<INodeHandler, bool> func)
    {
        var visited = new HashSet<INodeHandler>();
        var queueNodes = new Queue<INodeHandler>();
        queueNodes.Enqueue(this);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add(node))
            {
                continue;
            }

            if (!func(node))
            {
                return;
            }

            foreach (var outputProperty in node.Outputs)
            {
                foreach (var connection in outputProperty.Connections)
                {
                    queueNodes.Enqueue(connection.Node);
                }
            }
        }
    }
}
