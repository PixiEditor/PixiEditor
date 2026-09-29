using System.Net.WebSockets;
using Avalonia.Controls.Documents;
using Avalonia.Threading;
using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.WasmRuntime.Utilities;
using ProtoBuf;
using WebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;

namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

internal class NetworkModule(WasmExtensionInstance extension) : ApiModule(extension), INetworkProvider
{
    HttpClient httpClient = new();

    private Dictionary<int, ClientWebSocket> webSockets = new();
    private int nextWebSocketId = 1;


    public async AsyncCall<Response> SendRequest(Request request)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Parse(request.Method), request.Url);

        foreach (var header in request.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Body?.Length > 0)
        {
            httpRequest.Content = new ByteArrayContent(request.Body);
            if (!string.IsNullOrWhiteSpace(request.ContentType))
            {
                httpRequest.Content.Headers.ContentType =
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse(request.ContentType);
            }
        }

        try
        {
            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);
            byte[] responseBody = await httpResponse.Content.ReadAsByteArrayAsync();

            Response response = new()
            {
                Url = httpResponse.RequestMessage?.RequestUri?.ToString() ?? request.Url,
                StatusCode = (int)httpResponse.StatusCode, Body = responseBody,
            };

            foreach (var header in httpResponse.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }

            foreach (var header in httpResponse.Content.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }

            return response;
        }
        catch (Exception ex)
        {
            return new Response
            {
                Url = request.Url,
                StatusCode = 0, Body = Array.Empty<byte>(), Headers = { ["Error"] = ex.Message }
            };
        }
    }

    public async AsyncCall<int> WebSocketConnect(WebSocketRequest request)
    {
        var webSocket = new ClientWebSocket();
        int webSocketId = nextWebSocketId++;
        webSockets[webSocketId] = webSocket;

        await webSocket.ConnectAsync(new Uri(request.Url), CancellationToken.None);
        Dispatcher.UIThread.Post(() =>
        {
            RunMessenger(webSocketId, webSocket, message => PassMessage(webSocketId, message), () => WebSocketClosed(webSocketId));
        });

        return webSocketId;
    }

    private void WebSocketClosed(int webSocketId)
    {
        Extension.Instance?.GetAction<int>("websocket_on_closed")?.Invoke(webSocketId);
    }

    public async AsyncCall WebSocketSend<T>(int connectionId, WebSocketMessage message)
    {
        var webSocket = webSockets[connectionId];
        await webSocket.SendAsync(message.Body,
            message.MessageType == CommonApi.Network.WebSocketMessageType.Binary
                ? WebSocketMessageType.Binary
                : WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private void PassMessage(int id, WebSocketMessage response)
    {
        Dispatcher.UIThread.Post(() =>
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, response);
            var bytes = stream.ToArray();
            int ptr = Extension.WasmMemoryUtility.WriteBytes(bytes);
            try
            {
                Extension.Instance?.GetAction<int, int, int>("websocket_on_message_received")
                    ?.Invoke(id, ptr, bytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Network module was unable to pass websocket message to the extension: {Extension.Metadata.UniqueName}: " + ex);
            }
            finally
            {
                Extension.WasmMemoryUtility.Free(ptr);
            }
        });
    }

    private void RunMessenger(int webSocketId, ClientWebSocket webSocket, Action<WebSocketMessage> onMessageReceived, Action webSocketClosed)
    {
        Task.Run(async () =>
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                        CancellationToken.None);
                    webSockets.Remove(webSocketId);
                    webSocketClosed();
                }
                else
                {
                    var message = new byte[result.Count];
                    Array.Copy(buffer, message, result.Count);
                    var webSocketMessage = new WebSocketMessage
                    {
                        Body = message,
                        MessageType = result.MessageType == WebSocketMessageType.Binary
                            ? CommonApi.Network.WebSocketMessageType.Binary
                            : CommonApi.Network.WebSocketMessageType.Text
                    };
                    onMessageReceived(webSocketMessage);
                }
            }
        });
    }

    public void WebSocketClose(int connectionId)
    {
        if (webSockets.TryGetValue(connectionId, out var webSocket))
        {
            webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by user", CancellationToken.None);
            webSockets.Remove(connectionId);
        }
    }

    public bool IsWebSocketConnectionAlive(int connectionId)
    {
        if (webSockets.TryGetValue(connectionId, out var webSocket))
        {
            return webSocket.State == WebSocketState.Open;
        }

        return false;
    }
}
