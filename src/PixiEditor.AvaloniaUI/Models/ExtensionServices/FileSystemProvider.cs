using PixiEditor.AvaloniaUI.Views;
using PixiEditor.Extensions.IO;

namespace PixiEditor.AvaloniaUI.Models.ExtensionServices;

public class FileSystemProvider : IFileSystemProvider
{
    public bool OpenFileDialog(FileFilter filter, out string path)
    {
        var task = MainWindow.Current.StorageProvider.OpenFilePickerAsync(filter.ToAvaloniaFileFilters());
        task.Wait();

        if (task.Result == null || task.Result.Count == 0)
        {
            path = null;
            return false;
        }

        path = task.Result[0].Path.LocalPath;
        return true;
    }
}
