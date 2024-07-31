using Avalonia.Media;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Files;

internal class PixiFileType : IoFileType
{
    public static PixiFileType PixiFile { get; } = new PixiFileType();
    public override string DisplayName => new LocalizedString("PIXI_FILE");
    public override string[] Extensions => new[] { ".pixi" };

    public override SolidColorBrush EditorColor { get;  } = new SolidColorBrush(new Color(255, 226, 1, 45));

    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Pixi;

    public override async Task<SaveResult> TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config)
    {
        try
        {
            await Parser.PixiParser.V5.SerializeAsync(document.ToSerializable(), pathWithExtension);
        }
        catch (UnauthorizedAccessException e)
        {
            return SaveResult.SecurityError;
        }
        catch (IOException)
        {
            return SaveResult.IoError;
        }
        catch
        {
            return SaveResult.UnknownError;
        }

        return SaveResult.Success;
    }
}
