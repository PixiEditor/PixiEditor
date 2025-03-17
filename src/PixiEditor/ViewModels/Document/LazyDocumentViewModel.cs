using PixiEditor.Helpers;

namespace PixiEditor.ViewModels.Document;

internal class LazyDocumentViewModel : PixiObservableObject
{
    private string path;
    private bool associatePath;
    private Guid tempFileGuid;
    private string? autosavePath;
    private string? originalPath;

    public string Path
    {
        get => path;
        set => SetProperty(ref path, value);
    }

    public bool AssociatePath
    {
        get => associatePath;
        set => SetProperty(ref associatePath, value);
    }

    public Guid TempFileGuid
    {
        get => tempFileGuid;
        set => SetProperty(ref tempFileGuid, value);
    }

    public string? AutosavePath
    {
        get => autosavePath;
        set => SetProperty(ref autosavePath, value);
    }

    public string FileName => System.IO.Path.GetFileName(OriginalPath ?? Path);

    public string? OriginalPath
    {
        get => originalPath;
        set => SetProperty(ref originalPath, value);
    }

    public LazyDocumentViewModel(string path, bool associatePath)
    {
        Path = path;
        OriginalPath = path;
        AssociatePath = associatePath;
    }

    public void SetTempFileGuidAndLastSavedPath(Guid? tempGuid, string? tempPath)
    {
        if (tempGuid == null)
        {
            return;
        }

        TempFileGuid = tempGuid.Value;

        if (tempPath == null)
        {
            return;
        }

        AutosavePath = tempPath;
    }
}
