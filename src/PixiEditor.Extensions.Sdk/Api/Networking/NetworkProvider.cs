using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Sdk.Bridge;
using PixiEditor.Extensions.Sdk.Networking;

namespace PixiEditor.Extensions.Sdk.Api.Networking;

public class NetworkProvider : INetworkProvider
{
    public AsyncCall<Response> SendRequest(Request request)
    {
        return Interop.SendHttpRequest(request);
    }

    public AsyncCall<WebSocketConnection?> ConnectWebSocket(WebSocketRequest request)
    {
        return Interop.WebSocketConnect(request);
    }
}
