using Avalonia.Media;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.IO;
using Drawie.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.ExceptionHandling;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

internal class PixiFileType : IoFileType
{
    public static PixiFileType PixiFile { get; } = new PixiFileType();
    public override string DisplayName => new LocalizedString("PIXI_FILE");
    public override string[] Extensions => new[] { ".pixi" };

    public override SolidColorBrush EditorColor { get;  } = new SolidColorBrush(new Color(255, 226, 1, 45));

    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Pixi;

    public override async Task<SaveResult> TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config, ExportJob? job)
    {
        try
        {
            job?.Report(0, "Serializing document");
            await Parser.PixiParser.V5.SerializeAsync(document.ToSerializable(), pathWithExtension);
            job?.Report(1, "Document serialized");
        }
        catch (UnauthorizedAccessException e)
        {
            return SaveResult.SecurityError;
        }
        catch (IOException)
        {
            return SaveResult.IoError;
        }
        catch (Exception e)
        {
            CrashHelper.SendExceptionInfo(e);
            return SaveResult.UnknownError;
        }

        return SaveResult.Success;
    }
}
