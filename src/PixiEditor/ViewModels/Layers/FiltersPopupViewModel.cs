using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.Models.Handlers;
using PixiEditor.Parser.Graph;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Layers;

internal partial class FiltersPopupViewModel : ViewModelBase
{
    internal static List<FilterMetadata> AvailableFilters { get; }

    [ObservableProperty] public partial IStructureMemberHandler StructureMember { get; set; }

    private DocumentViewModel document;

    public DocumentViewModel Document
    {
        get => document;
        set
        {
            document?.NodeGraph.AllNodes.CollectionChanged -= OnNodeCollectionChanged;
            if(SetProperty(ref document, value))
            {
                document?.NodeGraph.AllNodes.CollectionChanged += OnNodeCollectionChanged;
            }
        }
    }

    public ObservableCollection<NodeViewModel> AddedFilters { get; } = new ObservableCollection<NodeViewModel>();

    [ObservableProperty]
    public partial NodeViewModel SelectedFilter { get; set; }

    static FiltersPopupViewModel()
    {
        AvailableFilters = new List<FilterMetadata>();
        var vms = typeof(NodeViewModel).Assembly.GetTypes()
            .Where(x => x.GetCustomAttribute<NodeViewModelAttribute>()?.Category == "FILTERS" && x.BaseType?.GenericTypeArguments.Length > 0);

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
    }

    [RelayCommand]
    public void AddFilter(FilterMetadata filter)
    {
        Guid lastFilterNode = FindNodeToConnectTo(StructureMember, out var inputPropName, out var position);

        var node = Document.NodeGraph.CreateNode(filter.NodeType, position - new VecD(250, 0));
        Document.NodeGraph.ConnectProperties(node, FilterNode.OutputPropertyName, lastFilterNode, inputPropName);
    }

    [RelayCommand]
    public void RemoveFilterCommand()
    {
        Document.NodeGraph.RemoveNode(SelectedFilter.Id, true);
    }

    private void OnNodeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (var eNewItem in e.NewItems)
            {
                if (eNewItem is NodeViewModel nvm && nvm.Category == "FILTERS" && AvailableFilters.Any(x => x.InternalName == nvm.InternalName))
                {
                    AddedFilters.Add(nvm);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (var eOldItem in e.OldItems)
            {
                if (eOldItem is NodeViewModel nvm)
                {
                    AddedFilters.Remove(nvm);
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            AddedFilters.Clear();
        }
    }

    private Guid FindNodeToConnectTo(IStructureMemberHandler structureMember, out string inputPropName, out VecD position)
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
