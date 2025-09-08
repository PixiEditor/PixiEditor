using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Helpers.Nodes;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.Views.Input;

namespace PixiEditor.Views.Nodes;

[TemplatePart("PART_InputBox", typeof(InputBox))]
[TemplatePart("PART_NodeList", typeof(ItemsControl))]
[TemplatePart("PART_ScrollViewer", typeof(ScrollViewer))]
public partial class NodePicker : TemplatedControl
{
    public static readonly StyledProperty<string> SearchQueryProperty = AvaloniaProperty.Register<NodePicker, string>(
        nameof(SearchQuery));

    public string SearchQuery
    {
        get => GetValue(SearchQueryProperty);
        set => SetValue(SearchQueryProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<NodeTypeInfo>> AllNodeTypeInfosProperty =
        AvaloniaProperty.Register<NodePicker, ObservableCollection<NodeTypeInfo>>(
            nameof(AllNodeTypeInfos));

    public static readonly StyledProperty<ObservableCollection<NodeTypeGroup>> FilteredNodeGroupsProperty =
        AvaloniaProperty.Register<NodePicker, ObservableCollection<NodeTypeGroup>>(nameof(FilteredNodeGroups));

    public static readonly StyledProperty<string> SelectedCategoryProperty =
        AvaloniaProperty.Register<NodePicker, string>(
            nameof(SelectedCategory));

    public string SelectedCategory
    {
        get => GetValue(SelectedCategoryProperty);
        set => SetValue(SelectedCategoryProperty, value);
    }

    public static readonly StyledProperty<NodeTypeInfo> SelectedNodeProperty =
        AvaloniaProperty.Register<NodePicker, NodeTypeInfo>(nameof(SelectedNode));

    public NodeTypeInfo? SelectedNode
    {
        get => GetValue(SelectedNodeProperty);
        set => SetValue(SelectedNodeProperty, value);
    }

    public ObservableCollection<NodeTypeInfo> AllNodeTypeInfos
    {
        get => GetValue(AllNodeTypeInfosProperty);
        set => SetValue(AllNodeTypeInfosProperty, value);
    }

    public ObservableCollection<NodeTypeGroup> FilteredNodeGroups
    {
        get => GetValue(FilteredNodeGroupsProperty);
        set => SetValue(FilteredNodeGroupsProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<string>> AllCategoriesProperty =
        AvaloniaProperty.Register<NodePicker, ObservableCollection<string>>(
            nameof(AllCategories));

    public static readonly StyledProperty<ObservableCollection<string>> FilteredCategoriesProperty =
        AvaloniaProperty.Register<NodePicker, ObservableCollection<string>>(
            nameof(FilteredCategories));

    public ObservableCollection<string> FilteredCategories
    {
        get => GetValue(FilteredCategoriesProperty);
        set => SetValue(FilteredCategoriesProperty, value);
    }

    public ObservableCollection<string> AllCategories
    {
        get => GetValue(AllCategoriesProperty);
        set => SetValue(AllCategoriesProperty, value);
    }

    public static readonly StyledProperty<ICommand> SelectNodeCommandProperty =
        AvaloniaProperty.Register<NodePicker, ICommand>(
            nameof(SelectNodeCommand));

    public ICommand SelectNodeCommand
    {
        get => GetValue(SelectNodeCommandProperty);
        set => SetValue(SelectNodeCommandProperty, value);
    }

    public Vector ScrollOffset
    {
        get { return (Vector)GetValue(ScrollOffsetProperty); }
        set { SetValue(ScrollOffsetProperty, value); }
    }

    public static readonly StyledProperty<Vector> ScrollOffsetProperty =
        AvaloniaProperty.Register<NodePicker, Vector>(nameof(ScrollOffset));

    private ItemsControl? _itemsControl;
    private ScrollViewer? _scrollViewer;
    private InputBox? _inputBox;

    private const string MiscCategory = "MISC";

    private bool SuppressCategoryChanged { get; set; }

    static NodePicker()
    {
        SearchQueryProperty.Changed.Subscribe(OnSearchQueryChanged);
        AllNodeTypeInfosProperty.Changed.Subscribe(OnAllNodeTypesChanged);
        SelectedCategoryProperty.Changed.Subscribe(SelectedCategoryChanged);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _inputBox = e.NameScope.Find<InputBox>("PART_InputBox");

        _inputBox.Loaded += (_, _) => _inputBox.SelectAll();
        _inputBox.KeyDown += OnInputBoxKeyDown;

        _itemsControl = e.NameScope.Find<ItemsControl>("PART_NodeList");
        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        _scrollViewer.ScrollChanged += Scrolled;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Dispatcher.UIThread.Post(() => _inputBox?.Focus(), DispatcherPriority.Input);
    }

    private void Scrolled(object? sender, ScrollChangedEventArgs e)
    {
        if (e.OffsetDelta.Y != 0)
        {
            if(_scrollViewer.ScrollBarMaximum.Y == 0)
            {
                return;
            }
            
            double normalizedY = ScrollOffset.Y / _scrollViewer.ScrollBarMaximum.Y;

            int index = (int)(normalizedY * _itemsControl.Items.Count);
            index = Math.Clamp(index, 0, _itemsControl.Items.Count - 1);
            string category = FilteredNodeGroups[index].Name;
            if (string.IsNullOrEmpty(category))
            {
                category = MiscCategory;
            }

            SuppressCategoryChanged = true;
            SelectedCategory = category;
            SuppressCategoryChanged = false;
        }
    }

    private static void OnSearchQueryChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not NodePicker nodePicker)
        {
            return;
        }

        nodePicker.SelectedNode = null;
        
        if (NodeAbbreviation.IsAbbreviation(nodePicker.SearchQuery, out var abbreviationName))
        {
            nodePicker.FilteredNodeGroups = nodePicker.NodeTypeGroupsFromQuery(abbreviationName);
            FilterCategories(nodePicker);
        }
        else
        {
            if (string.IsNullOrEmpty(nodePicker.SearchQuery))
            {
                nodePicker.FilteredCategories = new ObservableCollection<string>(nodePicker.AllCategories);

                nodePicker.FilteredNodeGroups =
                    nodePicker.NodeTypeGroupsFromQuery(null);
                
                FilterCategories(nodePicker);
            }
            else
            {
                nodePicker.FilteredNodeGroups = nodePicker.NodeTypeGroupsFromQuery(nodePicker.SearchQuery);

                FilterCategories(nodePicker);
            }
        }
        
        nodePicker.SelectedCategory = nodePicker.FilteredCategories.FirstOrDefault();
    }


    private ObservableCollection<NodeTypeGroup> NodeTypeGroupsFromQuery(string? query)
    {
        var filtered = 
            (!string.IsNullOrEmpty(query)
                ? AllNodeTypeInfos.Where(x => SearchComparer(x, query))
                : AllNodeTypeInfos).ToList();

        if (filtered.Count == 0) return new ObservableCollection<NodeTypeGroup>();

        var groups = new ObservableCollection<NodeTypeGroup>();
        foreach (var group in groups)
        {
            group.NodeTypes.Clear();
        }

        foreach (var info in filtered)
        {
            string category = string.IsNullOrEmpty(info.Category) ? MiscCategory : info.Category;
            var existingGroup = groups.FirstOrDefault(x => x.Name == category); 
            if (existingGroup == null)
            {
                existingGroup = new NodeTypeGroup(category, new List<NodeTypeInfo>());
                groups.Add(existingGroup);
            }

            existingGroup.NodeTypes.Add(info);
        }
        
        var miscGroup = groups.FirstOrDefault(x => x.Name == MiscCategory);
        if (miscGroup != null)
        {
            int index = groups.IndexOf(miscGroup);
            groups.Move(index, groups.Count - 1);
        }
        
        for (var i = 0; i < groups.Count; i++)
        {
            if (groups[i].NodeTypes.Count == 0)
            {
                groups.RemoveAt(i);
                i--;
            }
        }

        return groups;
    }

    private void OnInputBoxKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                HandleEnterDown(sender, e);
                return;
            case Key.Down or Key.Up:
                HandleKeyUpDown(sender, e); 
                return;
        }
    }

    private void HandleEnterDown(object? sender, KeyEventArgs e)
    {
        if (SelectedNode != null)
        {
            SelectNodeCommand.Execute(SelectedNode);
            return;
        }

        var nodes = NodeAbbreviation.FromString(SearchQuery, AllNodeTypeInfos);
        
        if(nodes == null || nodes.Count == 0)
        {
            return;
        }

        foreach (var node in nodes)
        {
            SelectNodeCommand.Execute(node);
        }
    }

    private void HandleKeyUpDown(object? sender, KeyEventArgs e)
    {
        if (SelectedNode == null)
        {
            SelectedNode = e.Key == Key.Down
                ? FilteredNodeGroups.FirstOrDefault()?.NodeTypes.FirstOrDefault()
                : FilteredNodeGroups.LastOrDefault()?.NodeTypes.LastOrDefault();
                
            return;
        }

        var direction = e.Key == Key.Down ? NextToDirection.Forwards : NextToDirection.Backwards;
        SelectedNode = GetNodeNextTo(FilteredNodeGroups, SelectedNode, direction, out var group);

        var container = _itemsControl.ContainerFromItem(group);
        var buttonList = container.FindDescendantOfType<ItemsControl>();
        
        var button = buttonList.ContainerFromItem(SelectedNode);

        const double padding = 2.6;
        const double paddingHeight = padding * 2 + 1;
        
        // Bring Button above/below also into view
        button.BringIntoView(new Rect(0, button.Bounds.Height * -padding, button.Bounds.Width, button.Bounds.Height * paddingHeight));
    }

    private static NodeTypeInfo? GetNodeNextTo(ObservableCollection<NodeTypeGroup> groups, NodeTypeInfo node, NextToDirection direction, out NodeTypeGroup group)
    {
        var currentGroup = groups.FirstOrDefault(x => x.NodeTypes.Contains(node));

        group = currentGroup;
        if (currentGroup == null)
            return null;
        
        var indexInGroup = currentGroup.NodeTypes.IndexOf(node);
        var groupIndex = groups.IndexOf(currentGroup);

        if (direction == NextToDirection.Backwards && indexInGroup == 0)
        {
            group = groups.WrapPreviousBeforeIndex(groupIndex);
            return group.NodeTypes.Last();
        }

        if (direction == NextToDirection.Forwards && indexInGroup == currentGroup.NodeTypes.Count - 1)
        {
            group = groups.WrapNextAfterIndex(groupIndex);
            return group.NodeTypes.First();
        }

        return currentGroup.NodeTypes[indexInGroup + (int)direction];
    }

    private static bool SearchComparer(NodeTypeInfo x, string lookFor) =>
        x.FinalPickerName.Value.Replace(" ", "")
            .Contains(lookFor.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);

    private static void OnAllNodeTypesChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NodePicker nodePicker)
        {
            nodePicker.FilteredNodeGroups = nodePicker.NodeTypeGroupsFromQuery(null);
            nodePicker.AllCategories = new ObservableCollection<string>(
                nodePicker.AllNodeTypeInfos.Select(x => x.Category)
                    .Where(x => !string.IsNullOrEmpty(x)).Distinct()); 

            nodePicker.FilteredNodeGroups = nodePicker.NodeTypeGroupsFromQuery(null); 
            FilterCategories(nodePicker);
            
            nodePicker.SelectedCategory = nodePicker.FilteredCategories.FirstOrDefault();
        }
    }

    private static void FilterCategories(NodePicker nodePicker)
    {
        nodePicker.FilteredCategories = new ObservableCollection<string>(nodePicker.AllCategories
            .Where(x => nodePicker.FilteredNodeGroups.Any(y => y.Name == x)));

        bool miscCategoryExists = nodePicker.FilteredNodeGroups.Any(x => x.Name == MiscCategory);
        if (miscCategoryExists)
        {
            nodePicker.FilteredCategories.Add(MiscCategory);
        }
    }

    private static void SelectedCategoryChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NodePicker nodePicker)
        {
            if (nodePicker.SuppressCategoryChanged || nodePicker._scrollViewer == null)
            {
                return;
            }

            int indexOfFirstItemInCategory = nodePicker.FilteredNodeGroups
                .Select((x, i) => (x, i))
                .FirstOrDefault(x => x.x.Name == nodePicker.SelectedCategory).i;

            double normalizedY = indexOfFirstItemInCategory / (double)nodePicker.FilteredNodeGroups.Count;

            double y = normalizedY * nodePicker._scrollViewer.ScrollBarMaximum.Y;

            if (double.IsNaN(y)) return;

            nodePicker.ScrollOffset = new Vector(0, y);
        }
    }
}
