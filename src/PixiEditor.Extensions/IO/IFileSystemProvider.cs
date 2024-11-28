namespace PixiEditor.Extensions.IO;

public interface IFileSystemProvider
{
    public bool OpenFileDialog(FileFilter filter, out string path);
}
