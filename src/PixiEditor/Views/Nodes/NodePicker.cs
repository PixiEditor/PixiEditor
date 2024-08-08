using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers.Nodes;
using PixiEditor.Models.Nodes;
using PixiEditor.Numerics;
using PixiEditor.Views.Input;

namespace PixiEditor.Views.Nodes;

[TemplatePart("PART_InputBox", typeof(InputBox))]
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var inputBox = e.NameScope.Find<InputBox>("PART_InputBox");
        
        inputBox.KeyDown += OnInputBoxKeyDown;
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
            nodePicker.FilteredNodeTypeInfos = new ObservableCollection<NodeTypeInfo>(nodePicker.AllNodeTypeInfos
                .Where(x => SearchComparer(x, nodePicker.SearchQuery)));
        }

        return;

        bool SearchComparer(NodeTypeInfo x, string lookFor) =>
            x.FinalPickerName.Value.Replace(" ", "")
                .Contains(lookFor.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);
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
        }
    }
}

