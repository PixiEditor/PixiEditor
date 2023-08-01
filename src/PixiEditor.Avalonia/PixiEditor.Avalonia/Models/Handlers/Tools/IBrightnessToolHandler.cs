using Avalonia.Input;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Containers.Tools;

internal interface IBrightnessToolHandler : IToolHandler
{
    public BrightnessMode BrightnessMode { get; set; }
    public int ToolSize { get; set; }
    public bool Darken { get; }
    public MouseButton UsedWith { get; set; }
    public int CorrectionFactor { get; }
}
