using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class KeyFrameGroupViewModel : KeyFrameViewModel, IKeyFrameGroupHandler
{
    public ObservableCollection<IKeyFrameHandler> Children { get; } = new ObservableCollection<IKeyFrameHandler>();

    public override int StartFrameBindable => Children.Count > 0 ? Children.Min(x => x.StartFrameBindable) : 0;
    public override int DurationBindable => Children.Count > 0 ? Children.Max(x => x.StartFrameBindable + x.DurationBindable) - StartFrameBindable : 0;

    public string LayerName => Document.StructureHelper.Find(LayerGuid).NodeNameBindable;

    public bool IsCollapsed
    {
        get => _isCollapsed;
        set
        {
            SetProperty(ref _isCollapsed, value);
            foreach (var child in Children)
            {
                if (child is KeyFrameViewModel keyFrame)
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
            if(child is KeyFrameViewModel keyFrame)
            {
                keyFrame.SetVisibility(isVisible);
            }
        }
        
        base.SetVisibility(isVisible);
    }

    public KeyFrameGroupViewModel(int startFrame, int duration, Guid layerGuid, Guid id, DocumentViewModel doc, DocumentInternalParts internalParts) 
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
    }

    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(StartFrameBindable));
        OnPropertyChanged(nameof(DurationBindable));
        
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems)
            {
                if (item is KeyFrameViewModel keyFrame)
                {
                    keyFrame.IsCollapsed = IsCollapsed;
                    keyFrame.SetVisibility(IsVisible);
                }
            }
        }
    }
}
