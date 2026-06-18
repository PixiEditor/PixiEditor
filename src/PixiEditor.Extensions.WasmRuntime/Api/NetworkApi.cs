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
}
