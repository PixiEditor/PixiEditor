using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.CommonApi.Tools;
using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Api.Resources;

namespace Sample13_Network;

public static class WebSocketSample
{
   public static void ConnectToWebSocket(PixiEditorApi api)
   {
       var request = new WebSocketRequest()
       {
           Url = "wss://echo.websocket.org", // Example WebSocket server
       };

       var connectionCall =
           api.NetworkProvider.ConnectWebSocket(request);
       connectionCall.Completed += connection =>
       {
           api.Logger.Log("WebSocket connection attempt completed.");
           if (connection != null)
           {
               api.Logger.Log("Connected to WebSocket server.");
               connection.OnMessageReceived += (message) =>
               {
                   var body = System.Text.Encoding.UTF8.GetString(message.Body);
                   api.Logger.Log("Received message: " + body);
               };

               // Send a test message
               var testMessage = new WebSocketMessage()
               {
                   Body = System.Text.Encoding.UTF8.GetBytes("Hello, WebSocket!"),
                   MessageType = WebSocketMessageType.Text,
               };
               connection.SendMessage(testMessage);
           }
           else
           {
               api.Logger.Log("Failed to connect to WebSocket server.");
           }
       };
   }
}