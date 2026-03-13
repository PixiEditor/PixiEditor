using PixiEditor.Extensions.CommonApi.Tools;

namespace PixiEditor.Extensions.WasmRuntime.Api.Tools;

public class PixiEditorExtensionToolConfig : ExtensionToolConfig
{
    public Extension Extension { get; }

    public PixiEditorExtensionToolConfig(CustomToolConfig config, Extension extension) : base(config)
    {
        Extension = extension;
    }
}
