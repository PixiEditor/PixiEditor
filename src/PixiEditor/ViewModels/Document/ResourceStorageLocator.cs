using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ViewModels.Document;

public class ResourceStorageLocator
{
    private Dictionary<int, string> mapper;
    private Dictionary<int, byte[]> resourceData;
    private Dictionary<int, object> instances;
    private string rootPath;

    internal ResourceStorageLocator(Dictionary<int, string> mapper, string rootPath,
        Dictionary<int, byte[]> resourceData)
    {
        this.mapper = mapper;
        this.rootPath = rootPath;
        this.resourceData = resourceData;
    }

    internal string GetFilePath(int handle)
    {
        return Path.Combine(rootPath, mapper[handle]);
    }

    internal byte[]? TryGetResourceData(int handle)
    {
        return resourceData.TryGetValue(handle, out var data) ? data : null;
    }

    internal T? TryGetInstanceOrLoad<T>(int handle, Func<byte[], T> loader) where T : class
    {
        if (instances == null)
            instances = new Dictionary<int, object>();

        if (instances.TryGetValue(handle, out var instance))
            return instance as T;

        var data = loader(TryGetResourceData(handle));
        instances[handle] = data;
        return data;
    }

    internal void RegisterInstance(int handle, DocumentViewModel doc)
    {
        if (instances == null)
            instances = new Dictionary<int, object>();

        instances[handle] = doc;
    }

    internal bool ContainsInstance(int handle)
    {
        return instances != null && instances.ContainsKey(handle);
    }

    public T GetInstance<T>(int handle)
    {
        return (T)instances[handle];
    }
}
