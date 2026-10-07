using PixiEditor.Extensions.CommonApi.Async;

namespace PixiEditor.Extensions.CommonApi.Network;

public interface INetworkProvider
{
    public AsyncCall<Response> SendRequest(Request request);
}
