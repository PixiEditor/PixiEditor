using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Networking;

public class WebSocketConnection
{
    public event Action<WebSocketMessage>? OnMessageReceived;
    public event Action OnConnectionClosed;
    internal int ConnectionHandle { get; private set; }

    public bool IsAlive => Native.is_websocket_connection_alive(ConnectionHandle);

    internal WebSocketConnection(int connectionHandle)
    {
        ConnectionHandle = connectionHandle;
    }

    internal void MessageReceived(WebSocketMessage request)
    {
        OnMessageReceived?.Invoke(request);
    }

    public void SendMessage(WebSocketMessage message)
    {
        Interop.SendWebSocketMessage(ConnectionHandle, message);
    }

    public void Close()
    {
        Native.websocket_close(ConnectionHandle);
    }

    internal void ConnectionClosed()
    {
        OnConnectionClosed?.Invoke();
    }
}
