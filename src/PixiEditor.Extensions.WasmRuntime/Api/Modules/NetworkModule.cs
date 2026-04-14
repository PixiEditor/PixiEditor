using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Network;
using PixiEditor.Extensions.Metadata;
using PixiEditor.Extensions.WasmRuntime.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

internal class NetworkModule(WasmExtensionInstance extension) : ApiModule(extension), INetworkProvider
{
    HttpClient httpClient = new();

    public async AsyncCall<Response> SendRequest(Request request)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Parse(request.Method), request.Url);

        foreach (var header in request.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Body?.Length > 0)
        {
            switch (request.ContentType)
            {
                case "application/json":
                    httpRequest.Content = new StringContent(System.Text.Encoding.UTF8.GetString(request.Body),
                        System.Text.Encoding.UTF8, "application/json");
                    break;
                case "application/x-www-form-urlencoded":
                    httpRequest.Content = new StringContent(System.Text.Encoding.UTF8.GetString(request.Body),
                        System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                    break;
                case "text/plain":
                    httpRequest.Content = new StringContent(System.Text.Encoding.UTF8.GetString(request.Body),
                        System.Text.Encoding.UTF8, "text/plain");
                    break;
                default:
                    httpRequest.Content = new ByteArrayContent(request.Body);
                    break;
            }
        }

        try
        {
            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);
            byte[] responseBody = await httpResponse.Content.ReadAsByteArrayAsync();

            Response response = new() { StatusCode = (int)httpResponse.StatusCode, Body = responseBody, };

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
            return new Response { StatusCode = 0, Body = Array.Empty<byte>(), Headers = { ["Error"] = ex.Message } };
        }
    }
}
