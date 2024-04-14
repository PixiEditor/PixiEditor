using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Tools;

namespace PixiEditor.AvaloniaUI.Models.Handlers.Tools;

internal interface IBrightnessToolHandler : IToolHandler
{
    public BrightnessMode BrightnessMode { get; }
    public bool Darken { get; }
    public MouseButton UsedWith { get; }
    public float CorrectionFactor { get; }
}
