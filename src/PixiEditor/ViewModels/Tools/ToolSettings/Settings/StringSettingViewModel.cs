using System.Collections.ObjectModel;
using Drawie.Backend.Core;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Controllers;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal class StringSettingViewModel : Setting<string>
{
    public StringSettingViewModel(string name, string label) : base(name)
    {
        Label = label;
    }
}
