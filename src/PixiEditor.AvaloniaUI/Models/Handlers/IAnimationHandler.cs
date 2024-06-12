using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IAnimationHandler
{
    public IReadOnlyCollection<IKeyFrameHandler> KeyFrames { get; }
    public int ActiveFrameBindable { get; set; }
    public void CreateRasterKeyFrame(Guid targetLayerGuid, int frame, bool cloneFromExisting);
    public void SetActiveFrame(int newFrame);
    internal void AddKeyFrame(IKeyFrameHandler keyFrame);
    internal void RemoveKeyFrame(Guid keyFrameId);
}
