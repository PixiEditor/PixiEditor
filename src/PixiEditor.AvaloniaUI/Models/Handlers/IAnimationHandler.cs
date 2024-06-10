using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IAnimationHandler
{
    public ObservableCollection<IClipHandler> Clips { get; }
    public void AddRasterClip(Guid targetLayerGuid, int frame, bool cloneFromExisting);
}
