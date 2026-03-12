using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Linq;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class CelGroupViewModel : CelViewModel, ICelGroupHandler
{
    private int? cachedStartFrame;
    private int? cachedDuration;
    public ObservableCollection<ICelHandler> Children { get; } = new ObservableCollection<ICelHandler>();

    public override int StartFrameBindable =>
        cachedStartFrame ??= (Children.Count > 0 ? Children.Min(x => x.StartFrameBindable) : 0);

    public override int DurationBindable => cachedDuration ??= (Children.Count > 0
        ? Children.Max(x => x.StartFrameBindable + x.DurationBindable) - StartFrameBindable
        : 0);

    public string LayerName => Document.StructureHelper.Find(LayerGuid).NodeNameBindable;

    public bool IsGroupSelected => Document?.SelectedStructureMember?.Id == LayerGuid;

    public bool IsCollapsed
    {
        get => _isCollapsed;
        set
        {
            SetProperty(ref _isCollapsed, value);
            foreach (var child in Children)
            {
                if (child is CelViewModel keyFrame)
                {
                    keyFrame.IsCollapsed = value;
                }
            }
        }
    }

    private bool _isCollapsed;

    public override void SetVisibility(bool isVisible)
    {
        foreach (var child in Children)
        {
            if (child is CelViewModel keyFrame)
            {
                keyFrame.SetVisibility(isVisible);
            }
        }

        base.SetVisibility(isVisible);
    }

    public CelGroupViewModel(int startFrame, int duration, Guid layerGuid, Guid id, DocumentViewModel doc,
        DocumentInternalParts internalParts)
        : base(startFrame, duration, layerGuid, id, doc, internalParts)
    {
        Children.CollectionChanged += ChildrenOnCollectionChanged;
        Document.StructureHelper.Find(LayerGuid).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(IStructureMemberHandler.NodeNameBindable))
            {
                OnPropertyChanged(nameof(LayerName));
            }
        };

        Document.PropertyChanged += DocumentOnPropertyChanged;
    }

    public bool IsKeyFrameAt(int frame)
    {
        foreach (var child in Children)
        {
            if (child is ICelGroupHandler group)
            {
                if (group.IsKeyFrameAt(frame))
                    return true;
            }
            else if (child.StartFrameBindable <= frame && frame < child.StartFrameBindable + child.DurationBindable)
            {
                return true;
            }
        }

        return false;
    }

    private void DocumentOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(IDocument.SelectedStructureMember))
        {
            OnPropertyChanged(nameof(IsGroupSelected));
        }
    }

    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        cachedStartFrame = null;
        cachedDuration = null;

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems)
            {
                if (item is CelViewModel cel)
                {
                    cel.IsCollapsed = IsCollapsed;
                    cel.SetVisibility(IsVisible);
                    cel.PropertyChanged += CelOnPropertyChanged;
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var item in e.OldItems)
            {
                if (item is CelViewModel cel)
                {
                    cel.PropertyChanged -= CelOnPropertyChanged;
                }
            }
        }

        OnPropertyChanged(nameof(StartFrameBindable));
        OnPropertyChanged(nameof(DurationBindable));
    }
    
    private void CelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ICelHandler.StartFrameBindable) or nameof(ICelHandler.DurationBindable))
        {
            cachedStartFrame = null;
            cachedDuration = null;
            OnPropertyChanged(nameof(StartFrameBindable));
            OnPropertyChanged(nameof(DurationBindable));
        }
    }
}
