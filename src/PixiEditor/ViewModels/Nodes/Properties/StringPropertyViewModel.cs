using System.ComponentModel;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class StringPropertyViewModel : NodePropertyViewModel<string>
{
    private string fileWatcherPath = string.Empty;
    private FileSystemWatcher fileWatcher;

    public RelayCommand OpenInDefaultAppCommand { get; }
    public RelayCommand OpenInFolderCommand { get; }

    public string StringValue
    {
        get => Value;
        set => Value = value;
    }

    public string Kind = "txt";

    public StringPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
        PropertyChanged += StringPropertyViewModel_PropertyChanged;
        OpenInDefaultAppCommand = new RelayCommand(OpenInDefaultApp);
        OpenInFolderCommand = new RelayCommand(OpenInFolder);
    }

    private void StringPropertyViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Value))
        {
            OnPropertyChanged(nameof(StringValue));
        }
    }

    private void OpenInDefaultApp()
    {
        try
        {
            if (!string.IsNullOrEmpty(fileWatcherPath) && File.Exists(fileWatcherPath))
            {
                OpenInDefaultApp(fileWatcherPath);
                return;
            }

            fileWatcherPath = CreateTempFile();
            CreateFileWatcher(fileWatcherPath);
            OpenInDefaultApp(fileWatcherPath);
        }
        catch (Exception ex)
        {
            NoticeDialog.Show(new LocalizedString("FAILED_TO_OPEN_EDITABLE_STRING_MESSAGE", ex.Message),
                "FAILED_TO_OPEN_EDITABLE_STRING_TITLE");
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    private void OpenInFolder()
    {
        if (!string.IsNullOrEmpty(fileWatcherPath) && File.Exists(fileWatcherPath))
        {
            IOperatingSystem.Current.OpenFolder(fileWatcherPath);
            return;
        }

        fileWatcherPath = CreateTempFile();
        CreateFileWatcher(fileWatcherPath);
        IOperatingSystem.Current.OpenFolder(fileWatcherPath);
    }

    private string CreateTempFile()
    {
        string extension = $".{Kind}";

        string dirPath = Path.Combine(Paths.TempFilesPath, "NodeProps");
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        string filePath = Path.Combine(dirPath, Guid.NewGuid().ToString("N") + extension);
        File.WriteAllText(filePath, StringValue);

        return filePath;
    }

    private void CreateFileWatcher(string filePath)
    {
        fileWatcher?.Dispose();
        fileWatcher = new FileSystemWatcher();
        fileWatcher.Path = Path.GetDirectoryName(filePath);
        fileWatcher.Filter = Path.GetFileName(filePath);
        fileWatcher.NotifyFilter = NotifyFilters.LastWrite;

        fileWatcher.Changed += (sender, args) =>
        {
            using FileStream stream = new(args.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new(stream);
            string text = reader.ReadToEnd();
            Dispatcher.UIThread.Post(() => StringValue = text);
        };

        fileWatcher.EnableRaisingEvents = true;
    }

    private void OpenInDefaultApp(string path)
    {
        IOperatingSystem.Current.OpenUri(path);
    }
}
