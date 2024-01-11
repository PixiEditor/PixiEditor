using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Files;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

public partial class ExportFilePopup : PixiEditorPopup
{
    public int SaveWidth
    {
        get => (int)GetValue(SaveWidthProperty);
        set => SetValue(SaveWidthProperty, value);
    }


    public int SaveHeight
    {
        get => (int)GetValue(SaveHeightProperty);
        set => SetValue(SaveHeightProperty, value);
    }

    public string? SavePath
    {
        get => (string)GetValue(SavePathProperty);
        set => SetValue(SavePathProperty, value);
    }

    public FileType SaveFormat
    {
        get => (FileType)GetValue(SaveFormatProperty);
        set => SetValue(SaveFormatProperty, value);
    }

    public static readonly StyledProperty<int> SaveHeightProperty =
        AvaloniaProperty.Register<ExportFilePopup, int>(nameof(SaveHeight), 32);

    public static readonly StyledProperty<int> SaveWidthProperty =
        AvaloniaProperty.Register<ExportFilePopup, int>(nameof(SaveWidth), 32);

    public static readonly StyledProperty<RelayCommand> SetBestPercentageCommandProperty =
        AvaloniaProperty.Register<ExportFilePopup, RelayCommand>(nameof(SetBestPercentageCommand));

    public static readonly StyledProperty<string?> SavePathProperty =
        AvaloniaProperty.Register<ExportFilePopup, string?>(nameof(SavePath), "");

    public static readonly StyledProperty<FileType> SaveFormatProperty =
        AvaloniaProperty.Register<ExportFilePopup, FileType>(nameof(SaveFormat), FileType.Png);

    public static readonly StyledProperty<AsyncRelayCommand> ExportCommandProperty =
        AvaloniaProperty.Register<ExportFilePopup, AsyncRelayCommand>(
            nameof(ExportCommand));

    public static readonly StyledProperty<string> SuggestedNameProperty = AvaloniaProperty.Register<ExportFilePopup, string>(
        nameof(SuggestedName));

    public string SuggestedName
    {
        get => GetValue(SuggestedNameProperty);
        set => SetValue(SuggestedNameProperty, value);
    }

    public AsyncRelayCommand ExportCommand
    {
        get => GetValue(ExportCommandProperty);
        set => SetValue(ExportCommandProperty, value);
    }

    public RelayCommand SetBestPercentageCommand
    {
        get => (RelayCommand)GetValue(SetBestPercentageCommandProperty);
        set => SetValue(SetBestPercentageCommandProperty, value);
    }

    public string SizeHint => new LocalizedString("EXPORT_SIZE_HINT", GetBestPercentage());

    public ExportFilePopup(int imageWidth, int imageHeight)
    {
        SaveWidth = imageWidth;
        SaveHeight = imageHeight;

        InitializeComponent();
        DataContext = this;
        Loaded += (_, _) => sizePicker.FocusWidthPicker();

        SaveWidth = imageWidth;
        SaveHeight = imageHeight;

        SetBestPercentageCommand = new RelayCommand(SetBestPercentage);
        ExportCommand = new AsyncRelayCommand(Export);
    }

    private async Task Export()
    {
        SavePath = await ChoosePath();
        if (SavePath != null)
        {
            Close(true);
        }
    }

    /// <summary>
    ///     Command that handles Path choosing to save file
    /// </summary>
    private async Task<string?> ChoosePath()
    {
        FilePickerSaveOptions options = new FilePickerSaveOptions
        {
            Title = new LocalizedString("EXPORT_SAVE_TITLE"),
            SuggestedFileName = SuggestedName,
            SuggestedStartLocation = await GetTopLevel(this).StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
            FileTypeChoices = SupportedFilesHelper.BuildSaveFilter(false),
            ShowOverwritePrompt = true
        };

        IStorageFile file = await GetTopLevel(this).StorageProvider.SaveFilePickerAsync(options);
        if (file != null)
        {
            if (string.IsNullOrEmpty(file.Name) == false)
            {
                SaveFormat = SupportedFilesHelper.GetSaveFileType(false, file);
                if (SaveFormat == FileType.Unset)
                {
                    return null;
                }

                string fileName = SupportedFilesHelper.FixFileExtension(file.Path.AbsolutePath, SaveFormat);

                return fileName;
            }
        }
        return null;
    }

    private int GetBestPercentage()
    {
        int maxDim = Math.Max(SaveWidth, SaveHeight);
        for (int i = 16; i >= 1; i--)
        {
            if (maxDim * i <= 1280)
                return i * 100;
        }

        return 100;
    }

    private void SetBestPercentage()
    {
        sizePicker.ChosenPercentageSize = GetBestPercentage();
        sizePicker.PercentageRb.IsChecked = true;
        sizePicker.PercentageLostFocus();
    }
}
