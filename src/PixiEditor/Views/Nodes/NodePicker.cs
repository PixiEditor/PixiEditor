using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
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
        if (e.Key != Key.Enter)
        {
            return;
        }

        var nodes = NodeAbbreviation.FromString(SearchQuery, AllNodeTypeInfos);
        
        if(nodes == null || nodes.Count == 0)
        {
            return;
        }

        if (nodes == null && FilteredNodeGroups.Count > 0)
        {
            SelectNodeCommand.Execute(FilteredNodeGroups[0]);
        }
        else
        {
            foreach (var node in nodes)
            {
                SelectNodeCommand.Execute(node);
            }
        }
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
