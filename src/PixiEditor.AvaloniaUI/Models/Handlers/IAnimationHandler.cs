using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IAnimationHandler
{
    public ObservableCollection<IKeyFrameHandler> KeyFrames { get; }
    public int ActiveFrameBindable { get; set; }
    public void AddRasterClip(Guid targetLayerGuid, int frame, bool cloneFromExisting);
    public void SetActiveFrame(int newFrame);
}
