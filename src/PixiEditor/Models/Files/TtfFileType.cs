using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

internal class TtfFileType : IoFileType
{
    public override string[] Extensions { get; } = new[] { ".ttf" };
    public override string DisplayName => new LocalizedString("TRUE_TYPE_FONT");
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Vector;

    public override bool CanSave => false;

    public override Task<SaveResult> TrySaveAsync(string pathWithExtension, DocumentViewModel document,
        ExportConfig config, ExportJob? job)
    {
        throw new NotSupportedException("Saving TTF files is not supported.");
    }

    public override SaveResult TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config,
        ExportJob? job)
    {
        throw new NotSupportedException("Saving TTF files is not supported.");
    }
}
