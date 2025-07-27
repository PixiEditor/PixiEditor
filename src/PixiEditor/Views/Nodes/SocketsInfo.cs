using Avalonia;
using PixiEditor.Models.Handlers;
using PixiEditor.Views.Nodes.Properties;

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
