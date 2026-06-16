using Avalonia.Media;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Models.IO;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

internal class PngFileType : ImageFileType
{
    public static PngFileType PngFile { get; } = new PngFileType();
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Image | FileTypeDialogDataSet.SetKind.Video;

    public override string DisplayName => new LocalizedString("PNG_FILE");
    public override EncodedImageFormat EncodedImageFormat { get; } = EncodedImageFormat.Png;
    public override string[] Extensions => new[] { ".png" };

    public override SolidColorBrush EditorColor { get; } = new SolidColorBrush(new Color(255, 56, 108, 254));

    public override async Task<SaveResult> TrySaveAsync(string pathWithExtension, DocumentViewModel document, ExportConfig exportConfig, ExportJob? job)
    {
        if(document.AnimationDataViewModel.KeyFrames.Count > 0 && exportConfig is { ExportAsSpriteSheet: false, AnimationRenderer: not null })
        {
            return await VideoFileType.SaveVideoAsync(pathWithExtension, document, exportConfig, job, true);
        }

        return await base.TrySaveAsync(pathWithExtension, document, exportConfig, job);
    }

    public override SaveResult TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config, ExportJob? job)
    {
        if(document.AnimationDataViewModel.KeyFrames.Count > 1 && config is { ExportAsSpriteSheet: false, AnimationRenderer: not null })
        {
            return VideoFileType.SaveVideo(pathWithExtension, document, config, job, true);
        }

        return base.TrySave(pathWithExtension, document, config, job);
    }
}
