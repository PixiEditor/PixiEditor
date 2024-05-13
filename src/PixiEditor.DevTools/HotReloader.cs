namespace PixiEditor.DevTools;

public class HotReloader
{
    public List<string> WatchedFiles { get; } = new();
    public Action<string>? OnFileChanged { get; set; }

    private List<FileSystemWatcher> _watchers = new();

    public void WatchFile(string path, string filter)
    {
        string directory = Path.GetDirectoryName(path);
        WatchedFiles.Add(path);

        FileSystemWatcher watcher = new(directory, filter);
        watcher.Changed += WatcherOnChanged;
        watcher.EnableRaisingEvents = true;
        _watchers.Add(watcher);
    }

    private void WatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            (sender as FileSystemWatcher).EnableRaisingEvents = false;
            OnFileChanged?.Invoke(e.FullPath);
        }

        finally
        {
            (sender as FileSystemWatcher).EnableRaisingEvents = true;
        }
    }
}
