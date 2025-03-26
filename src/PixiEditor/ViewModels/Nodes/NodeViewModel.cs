using System.Collections.ObjectModel;
using System.Reflection;
using Avalonia;
using Avalonia.Media;
using ChunkyImageLib;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using Drawie.Backend.Core;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Rendering;
using PixiEditor.Models.Structures;
using Drawie.Numerics;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.Nodes;

internal abstract class NodeViewModel : ObservableObject, INodeHandler
{
    private LocalizedString displayName;
    private IBrush? categoryBrush;
    private string? nodeNameBindable;
    private VecD position;
    private ObservableRangeCollection<INodePropertyHandler> inputs = new();
    private ObservableRangeCollection<INodePropertyHandler> outputs = new();
    private PreviewPainter resultPainter;
    private bool isSelected;
    private string? icon;

    protected Guid id;

    public Guid Id { get => id; private set => id = value; }

    public LocalizedString DisplayName
    {
        get => displayName;
        set
        {
            if (SetProperty(ref displayName, value) && nodeNameBindable == null)
            {
                OnPropertyChanged(nameof(NodeNameBindable));
            }
        }
    }

    public string Category { get; }

    public string NodeNameBindable
    {
        get => nodeNameBindable ?? DisplayName.Key;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
            {
                Internals.ActionAccumulator.AddFinishedActions(
                    new SetNodeName_Action(Id, value));
            }
        }
    }

    public string InternalName { get; private set; }

    public IBrush CategoryBackgroundBrush
    {
        get
        {
            if (categoryBrush == null)
            {
                if (!string.IsNullOrWhiteSpace(Category) &&
                    Application.Current.Styles.TryGetResource($"{Stylize(Category)}CategoryBackgroundBrush",
                        App.Current.ActualThemeVariant, out var brushObj) && brushObj is IBrush brush)
                {
                    categoryBrush = brush;
                }
            }

            return categoryBrush;

            string Stylize(string input) => string.Concat(input[0].ToString().ToUpper(), input.ToLower().AsSpan(1));
        }
    }

    public NodeMetadata? Metadata { get; set; }

    public VecD PositionBindable
    {
        get => position;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
            {
                Internals.ActionAccumulator.AddFinishedActions(
                    new NodePosition_Action([Id], value),
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

    public PreviewPainter ResultPainter
    {
        get => resultPainter;
        set => SetProperty(ref resultPainter, value);
    }

    public bool IsNodeSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    internal DocumentViewModel Document { get; private set; }
    internal DocumentInternalParts Internals { get; private set; }

    public void Initialize(Guid id, string internalName, DocumentViewModel document, DocumentInternalParts internals)
    {
        Id = id;
        InternalName = internalName;
        Document = document;
        Internals = internals;
    }

    public virtual void OnInitialized() { }

    public NodeViewModel()
    {
        var attribute = GetType().GetCustomAttribute<NodeViewModelAttribute>();

        displayName = attribute.DisplayName;
        Category = attribute.Category;
    }

    public NodeViewModel(string nodeNameBindable, Guid id, VecD position, DocumentViewModel document,
        DocumentInternalParts internals)
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
        nodeNameBindable = new LocalizedString(newName);
        OnPropertyChanged(nameof(NodeNameBindable));
    }

    public string Icon => icon ??= GetType().GetCustomAttribute<NodeViewModelAttribute>().Icon;

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

    public void TraverseForwards(Func<INodeHandler, INodeHandler, INodePropertyHandler, INodePropertyHandler, bool> func)
    {
        var visited = new HashSet<INodeHandler>();
        var queueNodes = new Queue<(INodeHandler, INodeHandler, INodePropertyHandler, INodePropertyHandler)>();
        queueNodes.Enqueue((this, null, null, null));

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();

            if (!visited.Add(node.Item1))
            {
                continue;
            }

            if (!func(node.Item1, node.Item2, node.Item3, node.Item4))
            {
                return;
            }

            foreach (var outputProperty in node.Item1.Outputs)
            {
                foreach (var connection in outputProperty.ConnectedInputs)
                {
                    queueNodes.Enqueue((connection.Node, node.Item1, outputProperty, connection));
                }
            }
        }
    }

    public NodePropertyViewModel FindInputProperty(string propName)
    {
        return Inputs.FirstOrDefault(x => x.PropertyName == propName) as NodePropertyViewModel;
    }

    public NodePropertyViewModel<T> FindInputProperty<T>(string propName)
    {
        return Inputs.FirstOrDefault(x => x.PropertyName == propName) as NodePropertyViewModel<T>;
    }

    public NodePropertyViewModel FindOutputProperty(string propName)
    {
        return Outputs.FirstOrDefault(x => x.PropertyName == propName) as NodePropertyViewModel;
    }

    public NodePropertyViewModel<T> FindOutputProperty<T>(string propName)
    {
        return Outputs.FirstOrDefault(x => x.PropertyName == propName) as NodePropertyViewModel<T>;
    }
}

internal abstract class NodeViewModel<T> : NodeViewModel where T : Node
{
}
