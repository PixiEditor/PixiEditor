using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Networking;

public class WebSocketConnection
{
    public event EventHandler<WebSocketMessage>? OnMessageReceived;
    internal int ConnectionHandle { get; private set; }
    internal WebSocketConnection(int connectionHandle)
    {
        ConnectionHandle = connectionHandle;
    }

    internal void MessageReceived(WebSocketMessage request)
    {
        OnMessageReceived?.Invoke(this, request);
    }

    public void SendMessage(WebSocketMessage message)
    {
        Interop.SendWebSocketMessage(ConnectionHandle, message);
    }
}
