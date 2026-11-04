using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.Parser.Graph;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.ViewModels.Document.Nodes.FilterNodes;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Layers;

internal partial class FiltersPopupViewModel : ViewModelBase
{
    internal static List<FilterMetadata> AvailableFilters { get; }

    private IStructureMemberHandler structureMemberHandler;

    public IStructureMemberHandler StructureMember
    {
        get => structureMemberHandler;
        set
        {
            if (SetProperty(ref structureMemberHandler, value))
            {
                UpdateLayerFilters();
            }
        }
    }

    private DocumentViewModel document;

    public DocumentViewModel Document
    {
        get => document;
        set
        {
            UnsubscribeFromChanges();
            if (SetProperty(ref document, value))
            {
                document?.NodeGraph.Connections.CollectionChanged += OnNodeCollectionChanged;
                document?.LayersChanged += DocumentOnLayersChanged;
            }
        }
    }

    private void DocumentOnLayersChanged(object? sender, LayersChangedEventArgs e)
    {
        if (e.LayerAffectedGuid == StructureMember.Id && e.LayerChangeType == LayerAction.Remove)
        {
            StructureMember = null;
            StructureMemberInvalid?.Invoke();
        }
    }

    public ObservableCollection<NodeViewModel> LayerFilters { get; } = new ObservableCollection<NodeViewModel>();

    [ObservableProperty] public partial NodeViewModel SelectedFilter { get; set; }

    public event Action StructureMemberInvalid;

    static FiltersPopupViewModel()
    {
        AvailableFilters = new List<FilterMetadata>();
        var vms = typeof(NodeViewModel).Assembly.GetTypes()
            .Where(x => x.GetCustomAttribute<NodeViewModelAttribute>()?.Category == "FILTERS" &&
                        typeof(IFilterNodeHandler).IsAssignableFrom(x));

        foreach (var type in vms)
        {
            var attr = type.GetCustomAttribute<NodeViewModelAttribute>();
            Type targetType = type.BaseType?.GetGenericArguments().FirstOrDefault();
            if (targetType != null)
            {
                var internalName = targetType.GetCustomAttribute<NodeInfoAttribute>()?.UniqueName;
                AvailableFilters.Add(new FilterMetadata(internalName, attr.DisplayName, attr.Icon, targetType));
            }
        }
    }

    public FiltersPopupViewModel()
    {
        LayerFilters.CollectionChanged += LayerFiltersOnCollectionChanged;
    }

    private void LayerFiltersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            NodeViewModel next = e.NewStartingIndex == 0
                ? StructureMember as NodeViewModel
                : LayerFilters[e.NewStartingIndex - 1];
            NodeViewModel newItemAtIndex = LayerFilters[e.NewStartingIndex];
            ReorderNodes(next, newItemAtIndex);
        }
    }

    [RelayCommand]
    public void AddFilter(FilterMetadata filter)
    {
        if (Document.IsDisposed) return;

        Guid lastFilterNode = FindNodeToConnectTo(StructureMember, out var inputPropName, out var position);

        var node = Document.NodeGraph.CreateNode(filter.NodeType, position - new VecD(250, 0));
        Document.NodeGraph.ConnectProperties(node, FilterNode.OutputPropertyName, lastFilterNode, inputPropName);
    }

    [RelayCommand]
    public void RemoveFilterCommand()
    {
        if (!Document.IsDisposed)
        {
            Document.NodeGraph.RemoveNode(SelectedFilter.Id, true);
        }
    }

    public void UnsubscribeFromChanges()
    {
        document?.NodeGraph.Connections.CollectionChanged -= OnNodeCollectionChanged;
        document?.LayersChanged -= DocumentOnLayersChanged;
    }

    private void ReorderNodes(NodeViewModel oldItemAtIndex, NodeViewModel newItemAtIndex)
    {
        if (LayerFilters.Count == 0 || StructureMember == null)
            return;

        Document.Operations.MoveNode(newItemAtIndex.Id, oldItemAtIndex.Id);
    }

    private void OnNodeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateLayerFilters();
    }

    private void UpdateLayerFilters()
    {
        LayerFilters.Clear();
        if (StructureMember != null)
        {
            StructureMember.InputPropertyMap[StructureNode.FiltersPropertyName].ConnectedOutput?.Node
                .TraverseBackwards((
                    n) =>
                {
                    if (n is NodeViewModel nvm and IFilterNodeHandler)
                    {
                        LayerFilters.Add(nvm);
                    }

                    return Traverse.Further;
                });
        }
    }

    private Guid FindNodeToConnectTo(IStructureMemberHandler structureMember, out string inputPropName,
        out VecD position)
    {
        var filtersInput = structureMember.InputPropertyMap[StructureNode.FiltersPropertyName];
        position = structureMember.PositionBindable;

        if (filtersInput.ConnectedOutput != null)
        {
            INodeHandler node = filtersInput.ConnectedOutput.Node;
            Guid lastNode = structureMember.Id;
            while (node != null)
            {
                if (node.InputPropertyMap.TryGetValue(FilterNode.InputPropertyName, out var input))
                {
                    position = node.PositionBindable;
                    lastNode = node.Id;
                    node = input.ConnectedOutput?.Node;
                }
                else
                {
                    break;
                }
            }

            inputPropName = FilterNode.InputPropertyName;
            return lastNode;
        }

        inputPropName = StructureNode.FiltersPropertyName;
        return structureMember.Id;
    }
}

internal class FilterMetadata
{
    public string InternalName { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public Type NodeType { get; set; }

    public FilterMetadata(string internalName, string attrDisplayName, string? attrIcon, Type nodeType)
    {
        InternalName = internalName;
        Name = attrDisplayName;
        Icon = attrIcon;
        NodeType = nodeType;
    }
}
