using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Models.Files;

internal abstract class VideoFileType : IoFileType
{
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Video;
    public override async Task<SaveResult> TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config)
    {
        if (config.AnimationRenderer is null)
            return SaveResult.UnknownError;
        
        document.RenderFrames(Paths.TempRenderingPath, surface =>
        {
            if (config.ExportSize != surface.Size)
            {
                return surface.ResizeNearestNeighbor(config.ExportSize);
            }

            return surface;
        });
        
        var result = await config.AnimationRenderer.RenderAsync(Paths.TempRenderingPath, pathWithExtension);
        return result ? SaveResult.Success : SaveResult.UnknownError;
    }
}
