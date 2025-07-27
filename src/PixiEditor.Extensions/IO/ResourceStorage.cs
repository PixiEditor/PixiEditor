namespace PixiEditor.Extensions.IO;

public interface IResourceStorage
{
    public Stream GetResourceStream(string resourcePath);
    public bool Exists(string path);
}
