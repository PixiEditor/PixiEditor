using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

internal abstract class VideoFileType : IoFileType
{
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Video;

    public override async Task<SaveResult> TrySave(string pathWithExtension, DocumentViewModel document,
        ExportConfig config)
    {
        if (config.AnimationRenderer is null)
            return SaveResult.UnknownError;

        List<Image> frames = new(); 

        document.RenderFrames(frames, surface =>
        {
            if (config.ExportSize != surface.Size)
            {
                return surface.ResizeNearestNeighbor(config.ExportSize);
            }

            return surface;
        });

        var result = await config.AnimationRenderer.RenderAsync(frames, pathWithExtension);
        
        foreach (var frame in frames)
        {
            frame.Dispose();
        } 

        return result ? SaveResult.Success : SaveResult.UnknownError;
    }
}
