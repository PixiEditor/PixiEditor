using System.Collections.ObjectModel;
using ChunkyImageLib;

namespace PixiEditor.Models.Handlers;

internal interface ICelGroupHandler : ICelHandler
{
    public ObservableCollection<ICelHandler> Children { get; }
    public bool IsKeyFrameAt(int frame);
}
