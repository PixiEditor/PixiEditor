using System.Collections.ObjectModel;
using ChunkyImageLib;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IKeyFrameGroupHandler : IKeyFrameHandler
{
    public ObservableCollection<IKeyFrameHandler> Children { get; }
}
