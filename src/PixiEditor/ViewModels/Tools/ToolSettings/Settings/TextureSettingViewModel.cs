using System.Collections.ObjectModel;
using Drawie.Backend.Core;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Controllers;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal class TextureSettingViewModel : Setting<Texture>
{
    public TextureSettingViewModel(string name, string label) : base(name)
    {
        Label = label;
    }
}
