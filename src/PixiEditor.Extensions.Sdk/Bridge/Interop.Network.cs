using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Sdk.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    public static AsyncCall<Response> SendHttpRequest(CommonApi.Network.Request request)
    {
        using MemoryStream stream = new();
        Serializer.Serialize(stream, request);
        byte[] bytes = stream.ToArray();
        IntPtr ptr = InteropUtility.ByteArrayToIntPtr(bytes);
        int asyncCallHandle = Native.send_http_request(ptr, bytes.Length);

        return Native.CreateAsyncCall(asyncCallHandle, responseBytes =>
        {
            using MemoryStream responseStream = new(responseBytes);
            return Serializer.Deserialize<Response>(responseStream);
        });
    }
}
