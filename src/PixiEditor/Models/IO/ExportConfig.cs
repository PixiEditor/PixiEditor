using PixiEditor.AnimationRenderer.Core;
using PixiEditor.AnimationRenderer.FFmpeg;
using Drawie.Numerics;

namespace PixiEditor.Models.IO;

public class ExportConfig
{
   public VecI ExportSize { get; set; }
   public bool ExportAsSpriteSheet { get; set; } = false;
   public int SpriteSheetColumns { get; set; }
   public int SpriteSheetRows { get; set; }
   public IAnimationRenderer? AnimationRenderer { get; set; }
   
   public VectorExportConfig? VectorExportConfig { get; set; }
   public string ExportOutput { get; set; }

   public ExportConfig(VecI exportSize)
   {
        ExportSize = exportSize;
   }
}
