using Avalonia;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Views.Nodes;

public class SocketsInfo
{
    public Dictionary<string, INodePropertyHandler> Sockets { get; } = new();
    public Func<INodePropertyHandler, Point> GetSocketPosition { get; set; }

    public SocketsInfo(Func<INodePropertyHandler, Point> getSocketPosition)
    {
        GetSocketPosition = getSocketPosition;
    }
}
