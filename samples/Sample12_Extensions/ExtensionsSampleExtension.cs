using PixiEditor.Extensions.Sdk;

namespace Sample12_Extensions;

public class ExtensionsSampleExtension : PixiEditorExtension
{
    public override void OnInitialized()
    {
        Api.Logger.Log(string.Join(", ", Api.ExtensionsProvider.GetInstalled()));
    }
}