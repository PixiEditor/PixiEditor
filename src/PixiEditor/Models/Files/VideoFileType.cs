using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.Models.IO;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

internal abstract class VideoFileType : IoFileType
{
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Video;

    public override async Task<SaveResult> TrySaveAsync(string pathWithExtension, DocumentViewModel document,
        ExportConfig config, ExportJob? job)
    {
        return await SaveVideoAsync(pathWithExtension, document, config, job, false);
    }

    public override SaveResult TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config,
        ExportJob? job)
    {
        return SaveVideo(pathWithExtension, document, config, job, false);
    }

    internal static SaveResult SaveVideo(string pathWithExtension, DocumentViewModel document, ExportConfig config,
        ExportJob? job, bool unpremultiply)
    {
        if (config.AnimationRenderer is null)
            return new SaveResult(SaveResultType.CustomError, new LocalizedString("ERR_NO_RENDERER"));

        List<Image> frames = new();

        job?.Report(0, new LocalizedString("WARMING_UP"));

        int frameRendered = 0;
        int totalFrames = document.AnimationDataViewModel.GetLastVisibleFrame() - 1;

        document.RenderFrames(frames, surface =>
        {
            return ProcessFrame(config, job, unpremultiply, totalFrames, surface, ref frameRendered);
        }, config.ExportOutput);

        job?.Report(0.5, new LocalizedString("RENDERING_VIDEO"));
        CancellationToken token = job?.CancellationTokenSource.Token ?? CancellationToken.None;
        var result = config.AnimationRenderer.Render(frames, pathWithExtension, token, progress =>
        {
            job?.Report((progress / 100f) * 0.5f + 0.5, new LocalizedString("RENDERING_VIDEO"));
        });

        job?.Report(1, new LocalizedString("FINISHED"));

        foreach (var frame in frames)
        {
            frame.Dispose();
        }

        return result ? new SaveResult(SaveResultType.Success) : new SaveResult(SaveResultType.CustomError, new LocalizedString("ERR_RENDERING_FAILED"));
    }

    internal static async Task<SaveResult> SaveVideoAsync(string pathWithExtension, DocumentViewModel document, ExportConfig config,
        ExportJob? job, bool unpremultiply)
    {
        if (config.AnimationRenderer is null)
            return new SaveResult(SaveResultType.CustomError, new LocalizedString("ERR_NO_RENDERER"));

        List<Image> frames = new();

        job?.Report(0, new LocalizedString("WARMING_UP"));

        int frameRendered = 0;
        int totalFrames = document.AnimationDataViewModel.GetLastVisibleFrame() - 1;

        document.RenderFrames(frames, surface =>
        {
            return ProcessFrame(config, job, unpremultiply, totalFrames, surface, ref frameRendered);
        }, config.ExportOutput);

        job?.Report(0.5, new LocalizedString("RENDERING_VIDEO"));
        CancellationToken token = job?.CancellationTokenSource.Token ?? CancellationToken.None;
        var result = await config.AnimationRenderer.RenderAsync(frames, pathWithExtension, token, progress =>
        {
            job?.Report((progress / 100f) * 0.5f + 0.5, new LocalizedString("RENDERING_VIDEO"));
        });

        job?.Report(1, new LocalizedString("FINISHED"));

        foreach (var frame in frames)
        {
            frame.Dispose();
        }

        return result ? new SaveResult(SaveResultType.Success) : new SaveResult(SaveResultType.CustomError, new LocalizedString("ERR_RENDERING_FAILED"));
    }

    private static Surface ProcessFrame(ExportConfig config, ExportJob? job, bool unpremultiply, int totalFrames,
        Surface surface, ref int frameRendered)
    {
        job?.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        frameRendered++;
        job?.Report(((double)frameRendered / totalFrames) / 2,
            new LocalizedString("RENDERING_FRAME", frameRendered, totalFrames));

        var targetSurface = surface;
        if (unpremultiply && surface.ImageInfo.AlphaType != AlphaType.Unpremul)
        {
            targetSurface = new Surface(new ImageInfo(surface.Size.X, surface.Size.Y, surface.ImageInfo.ColorType, AlphaType.Unpremul));
            targetSurface.DrawingSurface.Canvas.DrawSurface(surface.DrawingSurface, 0, 0);
        }

        if (config.ExportSize != targetSurface.Size)
        {
            var resized = targetSurface.ResizeNearestNeighbor(config.ExportSize);
            if(targetSurface != surface)
                targetSurface.Dispose();

            return resized;
        }

        return targetSurface;
    }
}
