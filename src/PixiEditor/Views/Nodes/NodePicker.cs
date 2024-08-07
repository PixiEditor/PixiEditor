using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Models.Nodes;
using PixiEditor.Numerics;

namespace PixiEditor.Views.Nodes;

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
            "AllNodeTypes");

    public static readonly StyledProperty<ObservableCollection<NodeTypeInfo>> FilteredNodeTypeInfosProperty =
        AvaloniaProperty.Register<NodePicker, ObservableCollection<NodeTypeInfo>>(nameof(FilteredNodeTypeInfos));

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

    public static readonly StyledProperty<ICommand> SelectNodeCommandProperty = AvaloniaProperty.Register<NodePicker, ICommand>(
        nameof(SelectNodeCommand));

    public ICommand SelectNodeCommand
    {
        get => GetValue(SelectNodeCommandProperty);
        set => SetValue(SelectNodeCommandProperty, value);
    }
    
    static NodePicker()
    {
        SearchQueryProperty.Changed.Subscribe(OnSearchQueryChanged);
        AllNodeTypeInfosProperty.Changed.Subscribe(OnAllNodeTypesChanged);
    }

    private static void OnSearchQueryChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NodePicker nodePicker)
        {
            // nodePicker.FilteredNodeTypes = new ObservableCollection<NodeTypeInfo>(nodePicker.AllNodeTypes
            //     .Where(x => x.Name.ToLower().Contains(nodePicker.SearchQuery.ToLower())));
        }
    }
    
    private static void OnAllNodeTypesChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NodePicker nodePicker)
        {
            nodePicker.FilteredNodeTypeInfos = new ObservableCollection<NodeTypeInfo>(nodePicker.AllNodeTypeInfos);
        }
    }
}

