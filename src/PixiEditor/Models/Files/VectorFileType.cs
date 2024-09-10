using PixiEditor.Models.IO;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

internal abstract class VectorFileType : IoFileType
{
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Vector;

    public override Task<SaveResult> TrySave(string pathWithExtension, DocumentViewModel document, ExportConfig config, ExportJob? job)
    {
        throw new NotImplementedException(); 
    }
}
