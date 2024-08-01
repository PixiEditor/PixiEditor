using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
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

    public static readonly StyledProperty<ObservableCollection<Type>> AllNodeTypesProperty =
        AvaloniaProperty.Register<NodePicker, ObservableCollection<Type>>(
            "AllNodeTypes");

    public static readonly StyledProperty<ObservableCollection<Type>> FilteredNodeTypesProperty =
        AvaloniaProperty.Register<NodePicker, ObservableCollection<Type>>(nameof(FilteredNodeTypes));

    public ObservableCollection<Type> AllNodeTypes
    {
        get => GetValue(AllNodeTypesProperty);
        set => SetValue(AllNodeTypesProperty, value);
    }

    public ObservableCollection<Type> FilteredNodeTypes
    {
        get { return (ObservableCollection<Type>)GetValue(FilteredNodeTypesProperty); }
        set { SetValue(FilteredNodeTypesProperty, value); }
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
    }

    private static void OnSearchQueryChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is NodePicker nodePicker)
        {
            nodePicker.FilteredNodeTypes = new ObservableCollection<Type>(nodePicker.AllNodeTypes
                .Where(x => x.Name.ToLower().Contains(nodePicker.SearchQuery.ToLower())));
        }
    }
}

