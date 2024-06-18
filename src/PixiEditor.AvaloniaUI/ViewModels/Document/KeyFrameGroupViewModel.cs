using System.Collections.ObjectModel;
using System.Reactive.Linq;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class KeyFrameGroupViewModel : KeyFrameViewModel, IKeyFrameGroupHandler
{
    public ObservableCollection<IKeyFrameHandler> Children { get; } = new ObservableCollection<IKeyFrameHandler>();

    public override int StartFrameBindable => Children.Count > 0 ? Children.Min(x => x.StartFrameBindable) : 0;
    public override int DurationBindable => Children.Count > 0 ? Children.Max(x => x.StartFrameBindable + x.DurationBindable) - StartFrameBindable : 0;

    public string LayerName => Document.StructureHelper.Find(LayerGuid).NameBindable;

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
        Document.StructureHelper.Find(LayerGuid).PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(StructureMemberViewModel.NameBindable))
            {
                OnPropertyChanged(nameof(LayerName));
            }
        };
    }
}
