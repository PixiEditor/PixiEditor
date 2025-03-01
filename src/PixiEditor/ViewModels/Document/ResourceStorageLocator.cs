namespace PixiEditor.ViewModels.Document;

public class ResourceStorageLocator
{
    private Dictionary<int, string> mapper;
    private string rootPath;

    internal ResourceStorageLocator(Dictionary<int, string> mapper, string rootPath)
    {
        this.mapper = mapper;
        this.rootPath = rootPath;
    }

    internal string GetFilePath(int handle)
    {
        return Path.Combine(rootPath, mapper[handle]);
    }
}
