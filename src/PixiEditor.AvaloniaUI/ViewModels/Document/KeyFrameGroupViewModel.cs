using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class KeyFrameGroupViewModel : KeyFrameViewModel, IKeyFrameGroupHandler
{
    public ObservableCollection<IKeyFrameHandler> Children { get; } = new ObservableCollection<IKeyFrameHandler>();

    public override int StartFrameBindable => Children.Count > 0 ? Children.Min(x => x.StartFrameBindable) : 0;
    public override int DurationBindable => Children.Count > 0 ? Children.Max(x => x.StartFrameBindable + x.DurationBindable) : 0;

    public KeyFrameGroupViewModel(int startFrame, int duration, Guid layerGuid, Guid id, DocumentViewModel doc, DocumentInternalParts internalParts) 
        : base(startFrame, duration, layerGuid, id, doc, internalParts)
    {
    }
}
