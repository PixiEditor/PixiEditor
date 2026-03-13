using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.CommonApi.Tools;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Sdk;
using PixiEditor.Extensions.Sdk.Api.FlyUI;
using PixiEditor.Extensions.Sdk.Api.Resources;
using PixiEditor.Extensions.Sdk.Api.Window;

namespace Sample11_Brushes;

public class BrushesSampleExtension : PixiEditorExtension
{
    /// <summary>
    ///     This method is called when extension is initialized. After this method is called, you can use Api property to access PixiEditor API.
    /// </summary>
    public override void OnInitialized()
    {
        Api.ToolsProvider.RegisterBrushTool(Resources.ReadAllBytes("Resources/Doggo.pixi"), new ExtensionToolConfig(new CustomToolConfig()
        {
                Name = "Doggo",
                Icon = "/cat.svg",
                DefaultShortcut = new Shortcut(Key.H, KeyModifiers.Alt),
                ToolTip = "I'm a brush yo",
                ActionsDisplayConfigs =
                {
                    new ActionDisplayConfig()
                    {
                        Text = "Default action display.",
                    },
                    new ActionDisplayConfig()
                    {
                        Text = "Custom display with SHIFT held",
                        Modifiers = (int)KeyModifiers.Shift,
                    },
                },
        }));

        Api.ToolsProvider.AddToolToToolset("Doggo", "PIXEL_ART_TOOLSET");
    }
}