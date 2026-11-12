using System.Runtime.InteropServices;
using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Sdk.Networking;
using PixiEditor.Extensions.Sdk.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    private static Dictionary<int, WebSocketConnection> webSocketConnections = new();

    private static void OnWebSocketMessageReceived(int connectionHandle, WebSocketMessage request)
    {
        if (webSocketConnections.TryGetValue(connectionHandle, out var connection))
        {
            connection.MessageReceived(request);
        }
    }

    private static void OnWebSocketClosed(int connectionHandle)
    {
        if (webSocketConnections.TryGetValue(connectionHandle, out var connection))
        {
            connection.ConnectionClosed();
            webSocketConnections.Remove(connectionHandle);
        }
    }

    public static AsyncCall<Response> SendHttpRequest(CommonApi.Network.Request request)
    {
        using MemoryStream stream = new();
        Serializer.Serialize(stream, request);
        byte[] bytes = stream.ToArray();
        IntPtr ptr = InteropUtility.ByteArrayToIntPtr(bytes);
        int asyncCallHandle = Native.send_http_request(ptr, bytes.Length);
        InteropUtility.FreeIntPtr(ptr);

        return Native.CreateAsyncCall(asyncCallHandle, responseBytes =>
        {
            using MemoryStream responseStream = new(responseBytes);
            return Serializer.Deserialize<Response>(responseStream);
        });
    }

    public static AsyncCall<WebSocketConnection?> WebSocketConnect(WebSocketRequest request)
    {
        using MemoryStream stream = new();
        Serializer.Serialize(stream, request);
        byte[] bytes = stream.ToArray();
        IntPtr ptr = InteropUtility.ByteArrayToIntPtr(bytes);
        int asyncCallHandle = Native.websocket_connect(ptr, bytes.Length);

        return Native.CreateAsyncCall(asyncCallHandle, responseBytes =>
        {
            int connectionHandle = BitConverter.ToInt32(responseBytes, 0);
            if (connectionHandle != -1)
            {
                var connection = new WebSocketConnection(connectionHandle);
                webSocketConnections[connectionHandle] = connection;
                return connection;
            }

            return null;
        });
    }

    public static void SendWebSocketMessage(int handle, WebSocketMessage message)
    {
        using MemoryStream stream = new();
        Serializer.Serialize(stream, message);
        byte[] bytes = stream.ToArray();
        IntPtr ptr = InteropUtility.ByteArrayToIntPtr(bytes);
        Native.websocket_send(handle, ptr, bytes.Length);
        InteropUtility.FreeIntPtr(ptr);
    }
}
