using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using Avalonia;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.Parser.Graph;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.ViewModels.Document.Nodes.FilterNodes;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Layers;

internal partial class FiltersPopupViewModel : ViewModelBase
{
    internal static List<FilterMetadata> AvailableFilters { get; }

    internal ObservableCollection<FilterMetadata> FilteredFilters { get; } = new ObservableCollection<FilterMetadata>();

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

    public ObservableCollection<FilterNodeViewModel> LayerFilters { get; } = new ObservableCollection<FilterNodeViewModel>();

    public FilterNodeViewModel? SelectedFilter
    {
        get => field;
        set
        {
            if(SetProperty(ref field, value))
            {
                DuplicateFilterCommand?.NotifyCanExecuteChanged();
                RemoveFilterCommand?.NotifyCanExecuteChanged();
            }
        }
    }

    [ObservableProperty]
    public partial FilterMetadata? SelectedPickerItem { get; set; }

    public string SearchText
    {
        get => field;
        set
        {
            if(SetProperty(ref field, value))
            {
                UpdateFilteredFilters();
            }
        }
    }

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
        FilteredFilters.AddRange(AvailableFilters);
    }

    private void UpdateFilteredFilters()
    {
        FilteredFilters.Clear();
        FilteredFilters.AddRange(AvailableFilters.Where(x => new LocalizedString(x.Name).Value.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        if (!FilteredFilters.Contains(SelectedPickerItem))
        {
            SelectedPickerItem = null;
        }
    }

    private void LayerFiltersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move)
        {
            NodeViewModel next = e.NewStartingIndex == 0
                ? StructureMember as NodeViewModel
                : LayerFilters[e.NewStartingIndex - 1].Node;
            NodeViewModel newItemAtIndex = LayerFilters[e.NewStartingIndex].Node;
            ReorderNodes(next, newItemAtIndex);
        }
    }

    [RelayCommand]
    public void AddFilter(FilterMetadata filter)
    {
        if (Document.IsDisposed) return;

        using var block = Document.Operations.StartChangeBlock();
        Guid lastFilterNode = FindNodeToConnectTo(StructureMember, out var inputPropName, out var position);

        var node = Document.NodeGraph.CreateNode(filter.NodeType, position - new VecD(250, 0));
        Document.NodeGraph.ConnectProperties(node, FilterNode.OutputPropertyName, lastFilterNode, inputPropName);
        Document.Operations.InvokeCustomAction(() =>
        {
            SelectedFilter = LayerFilters.FirstOrDefault(x => x.Node.Id == node);
        });
    }

    [RelayCommand(CanExecute = nameof(IsFilterSelected))]
    public void RemoveFilter()
    {
        if (!Document.IsDisposed && SelectedFilter != null)
        {
            int indexOf = LayerFilters.IndexOf(SelectedFilter);
            Document.NodeGraph.RemoveNode(SelectedFilter.Node.Id, true);
            Document.Operations.InvokeCustomAction(() =>
            {
                SelectedFilter = LayerFilters.ElementAtOrDefault(indexOf) ?? LayerFilters.LastOrDefault();
            });
        }
    }

    [RelayCommand(CanExecute = nameof(IsFilterSelected))]
    public void DuplicateFilter()
    {
        if (!Document.IsDisposed && SelectedFilter != null)
        {
            using var block = Document.Operations.StartChangeBlock();
            Guid? duplicated = Document.Operations.DuplicateNode(SelectedFilter.Node.Id);
            if (duplicated != null)
            {
                Document.Operations.MoveNode(duplicated.Value, SelectedFilter.Node.Id);
                Document.Operations.InvokeCustomAction(() =>
                {
                    SelectedFilter = LayerFilters.FirstOrDefault(x => x.Node.Id == duplicated.Value);
                });
            }
        }
    }

    private bool IsFilterSelected()
    {
        return SelectedFilter != null;
    }

    public void UnsubscribeFromChanges()
    {
        document?.NodeGraph.Connections.CollectionChanged -= OnNodeCollectionChanged;
        document?.LayersChanged -= DocumentOnLayersChanged;

        foreach (var filterNodeViewModel in LayerFilters)
        {
            filterNodeViewModel?.Unsubscribe();
        }
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
        List<NodeViewModel> newNodes = new();

        if (StructureMember != null)
        {
            StructureMember.InputPropertyMap[StructureNode.FiltersPropertyName].ConnectedOutput?.Node
                .TraverseBackwards(n =>
                {
                    if (n is NodeViewModel nvm && nvm is IFilterNodeHandler)
                        newNodes.Add(nvm);

                    return Traverse.Further;
                });
        }

        for (int i = LayerFilters.Count - 1; i >= 0; i--)
        {
            if (!newNodes.Contains(LayerFilters[i].Node))
            {
                LayerFilters[i].Unsubscribe();
                LayerFilters.RemoveAt(i);
            }
        }

        for (int i = 0; i < newNodes.Count; i++)
        {
            NodeViewModel node = newNodes[i];

            int existingIndex = LayerFilters
                .ToList()
                .FindIndex(f => ReferenceEquals(f.Node, node));

            if (existingIndex == i)
                continue;

            if (existingIndex >= 0)
            {
                FilterNodeViewModel existing = LayerFilters[existingIndex];
                LayerFilters.RemoveAt(existingIndex);
                LayerFilters.Insert(i, existing);
            }
            else
            {
                LayerFilters.Insert(i, new FilterNodeViewModel(node));
            }
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

internal partial class FilterNodeViewModel : ObservableObject
{
    [ObservableProperty]
    public partial NodeViewModel Node { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<INodePropertyHandler> InputsToEdit { get; set; } = new ObservableCollection<INodePropertyHandler>();

    private NodePropertyViewModel<bool> enabledProp;

    public FilterNodeViewModel(NodeViewModel node)
    {
        Node = node;
        enabledProp = node.FindInputProperty<bool>(FilterNode.IsFilterEnabledProperty);
        InputsToEdit.AddRange(node.Inputs.Where(x => x is not GenericPropertyViewModel && x.PropertyName != FilterNode.IsFilterEnabledProperty));
       if (enabledProp != null)
       {
           enabledProp.ValueChanged += PropOnValueChanged;
       }
    }

    private void PropOnValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        OnPropertyChanged(nameof(IsFilterEnabled));
    }

    public bool? IsFilterEnabled
    {
        get => (Node as IFilterNodeHandler).IsFilterEnabled;
        set => (Node as IFilterNodeHandler).IsFilterEnabled = value ?? true;
    }

    public void Unsubscribe()
    {
        enabledProp?.ValueChanged -= PropOnValueChanged;
    }
}
