using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.CommonApi.Tools;
using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Api.Resources;

namespace Sample12_Network;

public class NetworkSampleExtension : PixiEditorExtension
{
    /// <summary>
    ///     This method is called when extension is initialized. After this method is called, you can use Api property to access PixiEditor API.
    /// </summary>
    public override void OnInitialized()
    {
        Api.NetworkProvider.SendRequest(new Request()
        {
            Url = "https://jsonplaceholder.typicode.com/todos/1",
            Method = "GET",
        }).Completed += result =>
        {
            Api.Logger.Log("Status code: " + result.StatusCode);
            Api.Logger.Log("Response body: " + System.Text.Encoding.UTF8.GetString(result.Body));
        };
    }
}