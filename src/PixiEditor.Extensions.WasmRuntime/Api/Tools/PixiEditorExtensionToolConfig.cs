using PixiEditor.Extensions.CommonApi.Tools;
using PixiEditor.Extensions.CommonApi.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api.Tools;

public class PixiEditorExtensionToolConfig : ExtensionToolConfig
{
    public Extension Extension { get; }

    public PixiEditorExtensionToolConfig(CustomToolConfig config, Extension extension) : base(config)
    {
        Extension = extension;
        config.Name =
            PrefixedNameUtility.ToPixiEditorRelativePreferenceName(extension.Metadata.UniqueName, config.Name);
        config.ToolTip =
            PrefixedNameUtility.ToPixiEditorRelativePreferenceName(extension.Metadata.UniqueName, config.ToolTip);

        if (config.Icon.StartsWith("/"))
        {
            config.Icon = PrefixedNameUtility.ToPixiEditorRelativeResourcePath(extension.Metadata.UniqueName, config.Icon);
        }

        foreach (var configActionsDisplayConfig in config.ActionsDisplayConfigs)
        {
            configActionsDisplayConfig.Text =
                PrefixedNameUtility.ToPixiEditorRelativePreferenceName(extension.Metadata.UniqueName,
                    configActionsDisplayConfig.Text);
        }
    }
}
