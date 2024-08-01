using System.Collections.ObjectModel;
using Avalonia;
using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Structures;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.Nodes;
internal class NodeViewModel : ObservableObject, INodeHandler
{
    private string nodeNameBindable;
    private VecD position;
    private ObservableRangeCollection<INodePropertyHandler> inputs = new();
    private ObservableRangeCollection<INodePropertyHandler> outputs = new();
    private Surface resultPreview;
    private bool isSelected;

    protected Guid id;

    public Guid Id
    {
        get => id;
        init => id = value;
    }

    public string NodeNameBindable
    {
        get => nodeNameBindable;
        set
        {
            if (!Document.UpdateableChangeActive)
            {
                Internals.ActionAccumulator.AddFinishedActions(
                    new SetNodeName_Action(Id, value));
            }
        } 
    }

    public string InternalName { get; init; }

    public VecD PositionBindable
    {
        get => position;
        set
        {
            if (!Document.UpdateableChangeActive)
            {
                Internals.ActionAccumulator.AddFinishedActions(
                    new NodePosition_Action(Id, value),
                    new EndNodePosition_Action());
            }
        }
    }

    public ObservableRangeCollection<INodePropertyHandler> Inputs
    {
        get => inputs;
        set => SetProperty(ref inputs, value);
    }

    public ObservableRangeCollection<INodePropertyHandler> Outputs
    {
        get => outputs;
        set => SetProperty(ref outputs, value);
    }

    public Surface ResultPreview
    {
        get => resultPreview;
        set => SetProperty(ref resultPreview, value);
    }
    
    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    internal DocumentViewModel Document { get; init; }
    internal DocumentInternalParts Internals { get; init; }
    
    
    public NodeViewModel()
    {
        
    }

    public NodeViewModel(string nodeNameBindable, Guid id, VecD position, DocumentViewModel document, DocumentInternalParts internals)
    {
        this.nodeNameBindable = nodeNameBindable;
        this.id = id;
        this.position = position;
        Document = document;
        Internals = internals;
    }
    
    public void SetPosition(VecD newPosition)
    {
        position = newPosition;
        OnPropertyChanged(nameof(PositionBindable));
    }
    
    public void SetName(string newName)
    {
        nodeNameBindable = newName;
        OnPropertyChanged(nameof(NodeNameBindable));
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
                if (inputProperty.ConnectedOutput != null)
                {
                    queueNodes.Enqueue(inputProperty.ConnectedOutput.Node);
                }
            }
        }
    }

    public void TraverseBackwards(Func<INodeHandler, INodeHandler, bool> func)
    {
        var visited = new HashSet<INodeHandler>();
        var queueNodes = new Queue<(INodeHandler, INodeHandler)>();
        queueNodes.Enqueue((this, null));

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add(node.Item1))
            {
                continue;
            }
            
            if (!func(node.Item1, node.Item2))
            {
                return;
            }

            foreach (var inputProperty in node.Item1.Inputs)
            {
                if (inputProperty.ConnectedOutput != null)
                {
                    queueNodes.Enqueue((inputProperty.ConnectedOutput.Node, node.Item1));
                } 
            }
        }
    }

    public void TraverseBackwards(Func<INodeHandler, INodeHandler, INodePropertyHandler, bool> func)
    {
        var visited = new HashSet<INodeHandler>();
        var queueNodes = new Queue<(INodeHandler, INodeHandler, INodePropertyHandler)>();
        queueNodes.Enqueue((this, null, null));

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add(node.Item1))
            {
                continue;
            }
            
            if (!func(node.Item1, node.Item2, node.Item3))
            {
                return;
            }

            foreach (var inputProperty in node.Item1.Inputs)
            {
                if (inputProperty.ConnectedOutput != null)
                {
                    queueNodes.Enqueue((inputProperty.ConnectedOutput.Node, node.Item1, inputProperty));
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
                foreach (var connection in outputProperty.ConnectedInputs)
                {
                    queueNodes.Enqueue(connection.Node);
                }
            }
        }
    }
    
    public void TraverseForwards(Func<INodeHandler, INodeHandler, bool> func)
    {
        var visited = new HashSet<INodeHandler>();
        var queueNodes = new Queue<(INodeHandler, INodeHandler)>();
        queueNodes.Enqueue((this, null));

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add(node.Item1))
            {
                continue;
            }
            
            if (!func(node.Item1, node.Item2))
            {
                return;
            }

            foreach (var outputProperty in node.Item1.Outputs)
            {
                foreach (var connection in outputProperty.ConnectedInputs)
                {
                    queueNodes.Enqueue((connection.Node, node.Item1));
                }
            }
        }
    }
    
    public void TraverseForwards(Func<INodeHandler, INodeHandler, INodePropertyHandler, bool> func)
    {
        var visited = new HashSet<INodeHandler>();
        var queueNodes = new Queue<(INodeHandler, INodeHandler, INodePropertyHandler)>();
        queueNodes.Enqueue((this, null, null));

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add(node.Item1))
            {
                continue;
            }
            
            if (!func(node.Item1, node.Item2, node.Item3))
            {
                return;
            }

            foreach (var outputProperty in node.Item1.Outputs)
            {
                foreach (var connection in outputProperty.ConnectedInputs)
                {
                    queueNodes.Enqueue((connection.Node, node.Item1, outputProperty));
                }
            }
        }
    }

    public NodePropertyViewModel FindInputProperty(string propName)
    {
        return Inputs.FirstOrDefault(x => x.PropertyName == propName) as NodePropertyViewModel;
    }
    
    public NodePropertyViewModel FindOutputProperty(string propName)
    {
        return Outputs.FirstOrDefault(x => x.PropertyName == propName) as NodePropertyViewModel;
    }
}
