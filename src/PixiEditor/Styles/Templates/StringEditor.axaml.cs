using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Input;

[TemplatePart("PART_SmallTextBox", typeof(TextBox))]
[TemplatePart("PART_BigTextBox", typeof(TextBox))]
[TemplatePart("PART_ErrorScrollViewer", typeof(ScrollViewer))]
public class StringEditor : TemplatedControl
{
    public static readonly StyledProperty<string> ErrorsProperty = AvaloniaProperty.Register<StringEditor, string>(
        nameof(Errors));

    public string Errors
    {
        get => GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<StringEditor, string>(
        nameof(Text));

    public static readonly StyledProperty<string> ContentKindProperty = AvaloniaProperty.Register<StringEditor, string>(
        nameof(ContentKind), "txt");

    public string ContentKind
    {
        get => GetValue(ContentKindProperty);
        set => SetValue(ContentKindProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<ICommand> OpenInDefaultAppCommandProperty =
        AvaloniaProperty.Register<StringEditor, ICommand>(
            nameof(OpenInDefaultAppCommand));

    public ICommand OpenInDefaultAppCommand
    {
        get => GetValue(OpenInDefaultAppCommandProperty);
        set => SetValue(OpenInDefaultAppCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> OpenInFolderCommandProperty =
        AvaloniaProperty.Register<StringEditor, ICommand>(
            nameof(OpenInFolderCommand));

    public ICommand OpenInFolderCommand
    {
        get => GetValue(OpenInFolderCommandProperty);
        set => SetValue(OpenInFolderCommandProperty, value);
    }


    private TextBox bigTextBox;
    private TextBox smallTextBox;

    private string fileWatcherPath = string.Empty;
    private FileSystemWatcher fileWatcher;

    public StringEditor()
    {
        OpenInDefaultAppCommand = new RelayCommand(OpenInDefaultApp);
        OpenInFolderCommand = new RelayCommand(OpenInFolder);
    }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        bigTextBox = e.NameScope.Find<TextBox>("PART_BigTextBox");
        bigTextBox.PointerWheelChanged += BigTextBoxOnPointerWheelChanged;

        smallTextBox = e.NameScope.Find<TextBox>("PART_SmallTextBox");


        var errorScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ErrorScrollViewer");
        errorScrollViewer.PointerWheelChanged += BigTextBoxOnPointerWheelChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        ScrollViewer scroll = smallTextBox.FindDescendantOfType<ScrollViewer>();

        if (scroll is null)
        {
            return;
        }

        scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }

    private void BigTextBoxOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
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
        try
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
        catch (COMException ex)
        {
            NoticeDialog.Show(new LocalizedString("FAILED_TO_OPEN_EDITABLE_STRING_MESSAGE", ex.Message),
                "FAILED_TO_OPEN_EDITABLE_STRING_TITLE");
        }
        catch (Exception ex)
        {
            NoticeDialog.Show(new LocalizedString("FAILED_TO_OPEN_EDITABLE_STRING_MESSAGE", ex.Message),
                "FAILED_TO_OPEN_EDITABLE_STRING_TITLE");
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    private string CreateTempFile()
    {
        string extension = $".{ContentKind}";

        string dirPath = Path.Combine(Paths.TempFilesPath, "NodeProps");
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        string filePath = Path.Combine(dirPath, Guid.NewGuid().ToString("N") + extension);
        File.WriteAllText(filePath, Text);

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
            Dispatcher.UIThread.Post(() => Text = text);
        };

        fileWatcher.EnableRaisingEvents = true;
    }

    private void OpenInDefaultApp(string path)
    {
        IOperatingSystem.Current.OpenUri(path);
    }
}
