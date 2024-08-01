using System.Collections.ObjectModel;
using ChunkyImageLib;

namespace PixiEditor.Models.Handlers;

internal interface IKeyFrameGroupHandler : IKeyFrameHandler
{
    public ObservableCollection<IKeyFrameHandler> Children { get; }
}
