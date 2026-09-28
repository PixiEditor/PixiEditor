using System.Runtime.CompilerServices;
using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Sdk.Networking;
using PixiEditor.Extensions.Sdk.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    internal static event Action<int, WebSocketMessage> WebSocketMessageReceived;
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern int send_http_request(IntPtr ptr, int length);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern int websocket_connect(IntPtr ptr, int length);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void websocket_send(int handle, IntPtr ptr, int length);

    [ApiExport("websocket_on_message_received")]
    internal static void websocket_on_message_received(int asyncHandle, IntPtr ptr, int length)
    {
        byte[] bytes = InteropUtility.IntPtrToByteArray(ptr, length);
        WebSocketMessage request = Serializer.Deserialize<WebSocketMessage>(bytes.AsSpan());
        WebSocketMessageReceived?.Invoke(asyncHandle, request);
    }
}
