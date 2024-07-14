using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Nodes;

internal class NodeFrameViewModel : ObservableObject
{
    private Guid id;
    private VecD topLeft;
    private VecD bottomRight;
    private VecD size;
    
    public ObservableCollection<INodeHandler> Nodes { get; }

    public Guid Id
    {
        get => id;
        set => SetProperty(ref id, value);
    }
    
    public VecD TopLeft
    {
        get => topLeft;
        set => SetProperty(ref topLeft, value);
    }

    public VecD BottomRight
    {
        get => bottomRight;
        set => SetProperty(ref bottomRight, value);
    }

    public VecD Size
    {
        get => size;
        set => SetProperty(ref size, value);
    }

    public NodeFrameViewModel(Guid id, IEnumerable<INodeHandler> nodes)
    {
        Id = id;
        Nodes = new ObservableCollection<INodeHandler>(nodes);

        Nodes.CollectionChanged += OnCollectionChanged;
        AddHandlers(Nodes);

        CalculateBounds();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var action = e.Action;
        if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Replace && action != NotifyCollectionChangedAction.Reset)
        {
            return;
        }
        
        AddHandlers((IEnumerable<NodeViewModel>)e.NewItems);
        RemoveHandlers((IEnumerable<NodeViewModel>)e.OldItems);
    }

    private void AddHandlers(IEnumerable<INodeHandler> nodes)
    {
        foreach (var node in nodes)
        {
            node.PropertyChanged += NodePropertyChanged;
        }
    }

    private void RemoveHandlers(IEnumerable<INodeHandler> nodes)
    {
        foreach (var node in nodes)
        {
            node.PropertyChanged -= NodePropertyChanged;
        }
    }

    private void NodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(INodeHandler.PositionBindable))
        {
            return;
        }
        
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        if (Nodes.Count == 0)
        {
            if (TopLeft == BottomRight)
            {
                BottomRight = TopLeft + new VecD(100, 100);
            }
            
            return;
        }
        
        var minX = Nodes.Min(n => n.PositionBindable.X) - 30;
        var minY = Nodes.Min(n => n.PositionBindable.Y) - 45;
        
        var maxX = Nodes.Max(n => n.PositionBindable.X) + 130;
        var maxY = Nodes.Max(n => n.PositionBindable.Y) + 130;

        TopLeft = new VecD(minX, minY);
        BottomRight = new VecD(maxX, maxY);

        Size = BottomRight - TopLeft;
    }
}
