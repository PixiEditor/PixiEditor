using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Models.Files;

internal abstract class VideoFileType : IoFileType
{
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Video;
    public override SaveResult TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config)
    {
        if (config.AnimationRenderer is null)
            return SaveResult.UnknownError;
        
        document.RenderFrames(Paths.TempRenderingPath, surface =>
        {
            if (config.ExportSize is not null && config.ExportSize != surface.Size)
            {
                return surface.ResizeNearestNeighbor(config.ExportSize ?? surface.Size);
            }

            return surface;
        });
        
        config.AnimationRenderer.RenderAsync(Paths.TempRenderingPath, pathWithExtension);
        return SaveResult.Success;
    }
}
