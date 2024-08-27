using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PixiEditor.Helpers.Nodes;
using PixiEditor.Models.Nodes;
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

    public static readonly StyledProperty<ObservableCollection<NodeTypeInfo>> FilteredNodeTypeInfosProperty =
        AvaloniaProperty.Register<NodePicker, ObservableCollection<NodeTypeInfo>>(nameof(FilteredNodeTypeInfos));

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

    public ObservableCollection<NodeTypeInfo> FilteredNodeTypeInfos
    {
        get => GetValue(FilteredNodeTypeInfosProperty);
        set => SetValue(FilteredNodeTypeInfosProperty, value);
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

    private Dictionary<string, int> _categoryIndexes = new();

    public static readonly StyledProperty<Vector> ScrollOffsetProperty =
        AvaloniaProperty.Register<NodePicker, Vector>(nameof(ScrollOffset));

    private ItemsControl? _itemsControl;
    private ScrollViewer? _scrollViewer;

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
        var inputBox = e.NameScope.Find<InputBox>("PART_InputBox");

        inputBox.KeyDown += OnInputBoxKeyDown;

        _itemsControl = e.NameScope.Find<ItemsControl>("PART_NodeList");
        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        _scrollViewer.ScrollChanged += Scrolled;
    }

    private void Scrolled(object? sender, ScrollChangedEventArgs e)
    {
        if (e.OffsetDelta.Y != 0)
        {
            double normalizedY = ScrollOffset.Y / _scrollViewer.ScrollBarMaximum.Y;

            int index = (int)(normalizedY * _itemsControl.Items.Count);
            index = Math.Clamp(index, 0, _itemsControl.Items.Count - 1);
            string category = FilteredNodeTypeInfos[index].Category;
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
            nodePicker.FilteredNodeTypeInfos = new ObservableCollection<NodeTypeInfo>(nodePicker.AllNodeTypeInfos
                .Where(x => SearchComparer(x, abbreviationName)));
        }
        else
        {
            if (string.IsNullOrEmpty(nodePicker.SearchQuery))
            {
                nodePicker.FilteredCategories = new ObservableCollection<string>(nodePicker.AllCategories);
                UpdateCategoryDict(nodePicker);
                
                nodePicker.FilteredNodeTypeInfos =
                    OrderByCategory(nodePicker);
            }
            else
            {
                nodePicker.FilteredNodeTypeInfos = new ObservableCollection<NodeTypeInfo>(nodePicker.AllNodeTypeInfos
                    .Where(x => SearchComparer(x, nodePicker.SearchQuery)));
                
                FilterCategories(nodePicker);
            }
        }

        return;

        bool SearchComparer(NodeTypeInfo x, string lookFor) =>
            x.FinalPickerName.Value.Replace(" ", "")
                .Contains(lookFor.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);
    }

    private static void UpdateCategoryDict(NodePicker nodePicker)
    {
        nodePicker._categoryIndexes = nodePicker.FilteredCategories
            .Select((x, i) => (x, i))
            .ToDictionary(x => x.x, x => x.i);
    }

    private static ObservableCollection<NodeTypeInfo> OrderByCategory(NodePicker nodePicker)
    {
        return new ObservableCollection<NodeTypeInfo>(nodePicker.AllNodeTypeInfos
            .Where(x => x.Category != null)
            .OrderBy(
                x => string.IsNullOrEmpty(x.Category)
                    ? nodePicker._categoryIndexes[MiscCategory]
                    : nodePicker._categoryIndexes[x.Category]));
    }

    private void OnInputBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        var nodes = NodeAbbreviation.FromString(SearchQuery, AllNodeTypeInfos);

        if (nodes == null && FilteredNodeTypeInfos.Count > 0)
        {
            SelectNodeCommand.Execute(FilteredNodeTypeInfos[0]);
        }
        else
        {
            foreach (var node in nodes)
            {
                SelectNodeCommand.Execute(node);
            }
        }
    }

    private static void OnAllNodeTypesChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NodePicker nodePicker)
        {
            nodePicker.FilteredNodeTypeInfos = new ObservableCollection<NodeTypeInfo>(nodePicker.AllNodeTypeInfos);
            nodePicker.AllCategories = new ObservableCollection<string>(
                nodePicker.AllNodeTypeInfos.Select(x => x.Category)
                    .Where(x => !string.IsNullOrEmpty(x)).Distinct());

            nodePicker.AllCategories.Add(MiscCategory);

            nodePicker.FilteredCategories = new ObservableCollection<string>(nodePicker.AllCategories);

            UpdateCategoryDict(nodePicker); 

            nodePicker.FilteredNodeTypeInfos = OrderByCategory(nodePicker);
        }
    }
    
    private static void FilterCategories(NodePicker nodePicker)
    {
        nodePicker.FilteredCategories = new ObservableCollection<string>(nodePicker.AllCategories
            .Where(x => nodePicker.FilteredNodeTypeInfos.Any(y => y.Category == x)));
        
        bool miscCategoryExists = nodePicker.FilteredNodeTypeInfos.Any(x => string.IsNullOrEmpty(x.Category));
        if (miscCategoryExists)
        {
            nodePicker.FilteredCategories.Add(MiscCategory);
        }
        
        UpdateCategoryDict(nodePicker); 
    }

    private static void SelectedCategoryChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NodePicker nodePicker)
        {
            if (nodePicker.SuppressCategoryChanged)
            {
                return;
            }

            int indexOfFirstItemInCategory = nodePicker.FilteredNodeTypeInfos
                .Select((x, i) => (x, i))
                .FirstOrDefault(x => x.x.Category == nodePicker.SelectedCategory).i;

            double normalizedY = indexOfFirstItemInCategory / (double)nodePicker.FilteredNodeTypeInfos.Count;

            double y = normalizedY * nodePicker._scrollViewer.ScrollBarMaximum.Y;
            
            if(double.IsNaN(y)) return;

            nodePicker.ScrollOffset = new Vector(0, y);
        }
    }
}
