using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Networking;

public class NetworkProvider : INetworkProvider
{
    public AsyncCall<Response> SendRequest(Request request)
    {
        return Interop.SendHttpRequest(request);
    }
}
