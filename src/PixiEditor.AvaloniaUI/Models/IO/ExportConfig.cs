using PixiEditor.AnimationRenderer.Core;
using PixiEditor.AnimationRenderer.FFmpeg;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.IO;

public class ExportConfig
{
   public static ExportConfig Empty { get; } = new ExportConfig();
   public VecI? ExportSize { get; set; }
   public IAnimationRenderer? AnimationRenderer { get; set; }
}
