using Drawie.Backend.Core.Vector;

namespace PixiEditor.Models.Handlers;

public interface IPathOverlayHandler : IHandler
{
    public void Show(VectorPath path);
}
