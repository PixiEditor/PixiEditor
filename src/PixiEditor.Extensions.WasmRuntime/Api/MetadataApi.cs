namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class MetadataApi : ApiGroupHandler
{
    [ApiFunction("get_extension_unique_name")]
    public string GetExtensionUniqueName()
    {
        return Extension.Metadata.UniqueName;
    }
}
