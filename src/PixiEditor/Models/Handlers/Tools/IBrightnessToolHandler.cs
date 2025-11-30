using Avalonia.Input;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IBrightnessToolHandler : IBrushToolHandler
{
    public BrightnessMode BrightnessMode { get; }
    public bool Darken { get; }
    public MouseButton UsedWith { get; }
    public float CorrectionFactor { get; }
}
