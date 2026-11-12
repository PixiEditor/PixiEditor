using Drawie.Backend.Core;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal class TextureSettingViewModel : Setting<Texture>
{
    public TextureSettingViewModel(string name, string label) : base(name)
    {
        Label = label;
    }
}
