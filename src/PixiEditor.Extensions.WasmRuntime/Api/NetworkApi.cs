using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.WasmRuntime.Api.Modules;
using PixiEditor.Extensions.WasmRuntime.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class NetworkApi : ApiGroupHandler
{
    [ApiFunction("send_http_request")]
    internal int SendHttpRequest(Span<byte> request)
    {
        PermissionUtility.ThrowIfLacksPermissions(Extension.Metadata, ExtensionPermissions.Network, "SendHttpRequest");
        NetworkModule networkModule = Extension.GetModule<NetworkModule>();

        using MemoryStream stream = new();
        stream.Write(request);
        stream.Seek(0, SeekOrigin.Begin);
        Request deserializedRequest = Serializer.Deserialize<Request>(stream);

        var responseTask = networkModule.SendRequest(deserializedRequest);
        int asyncHandle = AsyncHandleManager.AddAsyncCall(responseTask, response =>
        {
            using MemoryStream responseStream = new();
            Serializer.Serialize(responseStream, response);
            return responseStream.ToArray();
        });
        return asyncHandle;
    }

    [ApiFunction("websocket_connect")]
    internal int WebSocketConnect(Span<byte> request)
    {
        PermissionUtility.ThrowIfLacksPermissions(Extension.Metadata, ExtensionPermissions.Network, "WebSocketConnect");
        NetworkModule networkModule = Extension.GetModule<NetworkModule>();

        using MemoryStream stream = new();
        stream.Write(request);
        stream.Seek(0, SeekOrigin.Begin);
        WebSocketRequest deserializedRequest = Serializer.Deserialize<WebSocketRequest>(stream);

        var responseTask = networkModule.WebSocketConnect(deserializedRequest);
        int asyncHandle = AsyncHandleManager.AddAsyncCall(responseTask, BitConverter.GetBytes);
        return asyncHandle;
    }

    [ApiFunction("websocket_send")]
    internal void WebSocketSend(int connectionId, Span<byte> request)
    {
        PermissionUtility.ThrowIfLacksPermissions(Extension.Metadata, ExtensionPermissions.Network, "WebSocketSend");
        NetworkModule networkModule = Extension.GetModule<NetworkModule>();

        using MemoryStream stream = new();
        stream.Write(request);
        stream.Seek(0, SeekOrigin.Begin);
        WebSocketMessage deserializedRequest = Serializer.Deserialize<WebSocketMessage>(stream);

        networkModule.WebSocketSend<WebSocketMessage>(connectionId, deserializedRequest);
    }

    [ApiFunction("websocket_close")]
    internal void WebSocketClose(int connectionId)
    {
        PermissionUtility.ThrowIfLacksPermissions(Extension.Metadata, ExtensionPermissions.Network, "WebSocketClose");
        NetworkModule networkModule = Extension.GetModule<NetworkModule>();

        networkModule.WebSocketClose(connectionId);
    }

    [ApiFunction("is_websocket_connection_alive")]
    internal bool IsWebSocketConnectionAlive(int connectionId)
    {
        PermissionUtility.ThrowIfLacksPermissions(Extension.Metadata, ExtensionPermissions.Network, "IsWebSocketConnectionAlive");
        NetworkModule networkModule = Extension.GetModule<NetworkModule>();

        return networkModule.IsWebSocketConnectionAlive(connectionId);
    }
}
