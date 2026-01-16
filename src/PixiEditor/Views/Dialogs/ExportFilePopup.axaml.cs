using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ChunkyImageLib;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using PixiEditor.Helpers;
using PixiEditor.Models.Files;
using PixiEditor.Models.IO;
using Drawie.Numerics;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;
using Image = Drawie.Backend.Core.Surfaces.ImageData.Image;

namespace PixiEditor.Views.Dialogs;

internal partial class ExportFilePopup : PixiEditorPopup
{
    public static readonly StyledProperty<int> SaveHeightProperty =
        AvaloniaProperty.Register<ExportFilePopup, int>(nameof(SaveHeight), 32);

    public static readonly StyledProperty<int> SaveWidthProperty =
        AvaloniaProperty.Register<ExportFilePopup, int>(nameof(SaveWidth), 32);

    public static readonly StyledProperty<RelayCommand> SetBestPercentageCommandProperty =
        AvaloniaProperty.Register<ExportFilePopup, RelayCommand>(nameof(SetBestPercentageCommand));

    public static readonly StyledProperty<string?> SavePathProperty =
        AvaloniaProperty.Register<ExportFilePopup, string?>(nameof(SavePath), "");

    public static readonly StyledProperty<IoFileType> SaveFormatProperty =
        AvaloniaProperty.Register<ExportFilePopup, IoFileType>(nameof(SaveFormat), new PngFileType());

    public static readonly StyledProperty<AsyncRelayCommand> ExportCommandProperty =
        AvaloniaProperty.Register<ExportFilePopup, AsyncRelayCommand>(
            nameof(ExportCommand));

    public static readonly StyledProperty<string> SuggestedNameProperty =
        AvaloniaProperty.Register<ExportFilePopup, string>(
            nameof(SuggestedName));

    public static readonly StyledProperty<Surface> ExportPreviewProperty =
        AvaloniaProperty.Register<ExportFilePopup, Surface>(
            nameof(ExportPreview));

    public static readonly StyledProperty<int> SelectedExportIndexProperty =
        AvaloniaProperty.Register<ExportFilePopup, int>(
            nameof(SelectedExportIndex), 0);

    public static readonly StyledProperty<bool> IsGeneratingPreviewProperty =
        AvaloniaProperty.Register<ExportFilePopup, bool>(
            nameof(IsGeneratingPreview), false);

    public static readonly StyledProperty<int> SpriteSheetColumnsProperty =
        AvaloniaProperty.Register<ExportFilePopup, int>(
            nameof(SpriteSheetColumns), 1);

    public static readonly StyledProperty<int> SpriteSheetRowsProperty =
        AvaloniaProperty.Register<ExportFilePopup, int>(
            nameof(SpriteSheetRows), 1);

    public static readonly StyledProperty<RenderOutputConfig> ExportOutputProperty =
        AvaloniaProperty.Register<ExportFilePopup, RenderOutputConfig>(
            nameof(ExportOutput));

    public static readonly StyledProperty<ObservableCollection<RenderOutputConfig>>
        AvailableExportOutputsProperty =
            AvaloniaProperty.Register<ExportFilePopup, ObservableCollection<RenderOutputConfig>>(
                nameof(AvailableExportOutputs));

    public static readonly StyledProperty<string> SizeHintProperty = AvaloniaProperty.Register<ExportFilePopup, string>(
        nameof(SizeHint));

    public static readonly StyledProperty<bool> FolderExportProperty = AvaloniaProperty.Register<ExportFilePopup, bool>(
        nameof(FolderExport));

    public bool FolderExport
    {
        get => GetValue(FolderExportProperty);
        set => SetValue(FolderExportProperty, value);
    }
    public string SizeHint
    {
        get => GetValue(SizeHintProperty);
        set => SetValue(SizeHintProperty, value);
    }

    public ObservableCollection<RenderOutputConfig> AvailableExportOutputs
    {
        get => GetValue(AvailableExportOutputsProperty);
        set => SetValue(AvailableExportOutputsProperty, value);
    }

    public RenderOutputConfig ExportOutput
    {
        get => GetValue(ExportOutputProperty);
        set => SetValue(ExportOutputProperty, value);
    }

    public int SpriteSheetRows
    {
        get => GetValue(SpriteSheetRowsProperty);
        set => SetValue(SpriteSheetRowsProperty, value);
    }

    public int SpriteSheetColumns
    {
        get => GetValue(SpriteSheetColumnsProperty);
        set => SetValue(SpriteSheetColumnsProperty, value);
    }

    public bool IsGeneratingPreview
    {
        get => GetValue(IsGeneratingPreviewProperty);
        set => SetValue(IsGeneratingPreviewProperty, value);
    }

    public int SelectedExportIndex
    {
        get => GetValue(SelectedExportIndexProperty);
        set => SetValue(SelectedExportIndexProperty, value);
    }

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

    public IoFileType SaveFormat
    {
        get => (IoFileType)GetValue(SaveFormatProperty);
        set => SetValue(SaveFormatProperty, value);
    }

    public Surface? ExportPreview
    {
        get => GetValue(ExportPreviewProperty);
        set => SetValue(ExportPreviewProperty, value);
    }

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

    public bool IsVideoExport => SelectedExportIndex == 1;

    public bool IsSpriteSheetExport => SelectedExportIndex == 2;

    public int AnimationPresetIndex
    {
        get { return (int)GetValue(AnimationPresetIndexProperty); }
        set { SetValue(AnimationPresetIndexProperty, value); }
    }

    public Array QualityPresetValues { get; }

    private DocumentViewModel document;
    private Image[]? videoPreviewFrames = [];
    private DispatcherTimer videoPreviewTimer = new DispatcherTimer();
    private int activeFrame = 0;
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private Task? generateSpriteSheetTask;

    public static readonly StyledProperty<int> AnimationPresetIndexProperty
        = AvaloniaProperty.Register<ExportFilePopup, int>("AnimationPresetIndex", 4);

    static ExportFilePopup()
    {
        SaveWidthProperty.Changed.Subscribe(RerenderPreview);
        SaveHeightProperty.Changed.Subscribe(RerenderPreview);
        SpriteSheetColumnsProperty.Changed.Subscribe(RerenderPreview);
        SpriteSheetRowsProperty.Changed.Subscribe(RerenderPreview);
        SelectedExportIndexProperty.Changed.Subscribe(RerenderPreview);
        ExportOutputProperty.Changed.Subscribe(UpdateOutput);
    }

    public ExportFilePopup(int imageWidth, int imageHeight, DocumentViewModel document)
    {
        SaveWidth = imageWidth;
        SaveHeight = imageHeight;
        QualityPresetValues = Enum.GetValues(typeof(QualityPreset));

        InitializeComponent();

        DataContext = this;
        Loaded += (_, _) => sizePicker.FocusWidthPicker();

        SetBestPercentageCommand = new RelayCommand(SetBestPercentage);
        ExportCommand = new AsyncRelayCommand(Export);
        this.document = document;
        videoPreviewTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(1000f / document.AnimationDataViewModel.FrameRateBindable)
        };
        videoPreviewTimer.Tick += OnVideoPreviewTimerOnTick;

        int framesCount = document.AnimationDataViewModel.GetVisibleFramesCount();

        var (rows, columns) = SpriteSheetUtility.CalculateGridDimensionsAuto(framesCount);
        SpriteSheetColumns = columns;
        SpriteSheetRows = rows;

        UpdateSizeHint();
        RenderPreview();
    }

    public void UpdateSizeHint()
    {
        SizeHint = new LocalizedString("EXPORT_SIZE_HINT", GetBestPercentage());
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (videoPreviewTimer != null)
        {
            videoPreviewTimer.Stop();
            videoPreviewTimer.Tick -= OnVideoPreviewTimerOnTick;
            videoPreviewTimer = null;
        }

        cancellationTokenSource?.Cancel();

        ExportPreview?.Dispose();

        if (videoPreviewFrames != null)
        {
            foreach (var frame in videoPreviewFrames)
            {
                frame?.Dispose();
            }
        }
    }

    private void OnVideoPreviewTimerOnTick(object? o, EventArgs eventArgs)
    {
        if (videoPreviewFrames.Length > 0)
        {
            ExportPreview.DrawingSurface.Canvas.Clear();
            if (videoPreviewFrames[activeFrame] == null)
            {
                return;
            }

            ExportPreview.DrawingSurface.Canvas.DrawImage(videoPreviewFrames[activeFrame], 0, 0);
            activeFrame = (activeFrame + 1) % videoPreviewFrames.Length;
        }
        else
        {
            videoPreviewTimer.Stop();
        }
    }

    private void RenderPreview()
    {
        if (document == null)
        {
            return;
        }

        IsGeneratingPreview = true;

        videoPreviewTimer.Stop();
        if (IsVideoExport)
        {
            StartRenderAnimationJob();
            videoPreviewTimer.Interval =
                TimeSpan.FromMilliseconds(1000f / document.AnimationDataViewModel.FrameRateBindable);
        }
        else
        {
            RenderImagePreview();
        }
    }

    private void RenderImagePreview()
    {
        try
        {
            if (IsSpriteSheetExport)
            {
                GenerateSpriteSheetPreview();
            }
            else
            {
                var exportOutput = ExportOutput;
                if (exportOutput == null || exportOutput.OriginalSize.ShortestAxis <= 0)
                {
                    return;
                }

                VecI size = new VecI(SaveWidth, SaveHeight);
                Task.Run(() =>
                {
                    var rendered = document.TryRenderWholeImage(0, size, exportOutput.OriginalSize,
                        exportOutput.Name);
                    if (rendered.IsT1)
                    {
                        VecI previewSize = CalculatePreviewSize(rendered.AsT1.Size);
                        Dispatcher.UIThread.Post(() =>
                        {
                            ExportPreview = rendered.AsT1.ResizeNearestNeighbor(previewSize);
                            rendered.AsT1.Dispose();
                        });
                    }
                });
            }
        }
        finally
        {
            IsGeneratingPreview = false;
        }
    }

    private void GenerateSpriteSheetPreview()
    {
        int clampedColumns = Math.Max(SpriteSheetColumns, 1);
        int clampedRows = Math.Max(SpriteSheetRows, 1);

        VecI previewSize = CalculatePreviewSize(new VecI(SaveWidth * clampedColumns, SaveHeight * clampedRows));
        VecI singleFrameSize = new VecI(previewSize.X / Math.Max(clampedColumns, 1),
            previewSize.Y / Math.Max(clampedRows, 1));
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = new CancellationTokenSource();

        if (generateSpriteSheetTask != null)
        {
            generateSpriteSheetTask.ContinueWith(x =>
            {
                Dispatcher.UIThread.Invoke(GenerateSpriteSheetPreview);
            });

            return;
        }

        ExportPreview?.Dispose();
        ExportPreview = Surface.ForDisplay(previewSize);

        string exportOutputName = ExportOutput.Name;
        generateSpriteSheetTask = Task.Run(
            () =>
            {
                try
                {
                    document.RenderFramesProgressive(
                        (frame, index) =>
                        {
                            int x = index % clampedColumns;
                            int y = index / clampedColumns;
                            var resized =
                                frame.ResizeNearestNeighbor(new VecI(singleFrameSize.X, singleFrameSize.Y));
                            DrawingBackendApi.Current.RenderingDispatcher.Invoke(() =>
                            {
                                if (ExportPreview.IsDisposed) return;
                                ExportPreview!.DrawingSurface.Canvas.DrawSurface(resized.DrawingSurface,
                                    x * singleFrameSize.X,
                                    y * singleFrameSize.Y);
                                resized.Dispose();
                            });
                        }, cancellationTokenSource.Token, exportOutputName);
                }
                catch
                {
                    // Ignore
                }
            }, cancellationTokenSource.Token).ContinueWith(x => generateSpriteSheetTask = null);
    }

    private void StartRenderAnimationJob()
    {
        if (cancellationTokenSource.Token is { CanBeCanceled: true })
        {
            cancellationTokenSource.Cancel();
        }

        cancellationTokenSource = new CancellationTokenSource();

        string exportOutputName = ExportOutput.Name;
        Task.Run(
            () =>
            {
                videoPreviewFrames =
                    document.RenderFrames(ProcessFrame, exportOutputName, cancellationTokenSource.Token);
            }, cancellationTokenSource.Token).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                VecI previewSize = CalculatePreviewSize(new VecI(SaveWidth, SaveHeight));
                if (previewSize != ExportPreview.Size)
                {
                    ExportPreview?.Dispose();
                    ExportPreview = Surface.ForDisplay(previewSize);
                }

                IsGeneratingPreview = false;
            });

            videoPreviewTimer?.Start();
        });
    }

    private Surface ProcessFrame(Surface surface)
    {
        return Dispatcher.UIThread.Invoke(() =>
        {
            Surface original = surface;
            if (SaveWidth != surface.Size.X || SaveHeight != surface.Size.Y)
            {
                original = surface.ResizeNearestNeighbor(new VecI(SaveWidth, SaveHeight));
                surface.Dispose();
            }

            VecI previewSize = CalculatePreviewSize(original.Size);
            if (previewSize != original.Size)
            {
                var resized = original.ResizeNearestNeighbor(previewSize);
                original.Dispose();
                return resized;
            }

            return original;
        });
    }

    private VecI CalculatePreviewSize(VecI imageSize)
    {
        VecI maxPreviewSize = new VecI(150, 200);
        if (imageSize.X > maxPreviewSize.X || imageSize.Y > maxPreviewSize.Y)
        {
            float scaleX = maxPreviewSize.X / (float)imageSize.X;
            float scaleY = maxPreviewSize.Y / (float)imageSize.Y;

            float scale = Math.Min(scaleX, scaleY);

            int newWidth = (int)(imageSize.X * scale);
            int newHeight = (int)(imageSize.Y * scale);

            newWidth = Math.Max(newWidth, 1);
            newHeight = Math.Max(newHeight, 1);

            return new VecI(newWidth, newHeight);
        }

        return imageSize;
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
        bool folderExport = FolderExport && SelectedExportIndex == 1;

        if (folderExport)
        {
            FolderPickerOpenOptions options = new FolderPickerOpenOptions()
            {
                Title = new LocalizedString("EXPORT_SAVE_TITLE"),
                SuggestedStartLocation = string.IsNullOrEmpty(document.FullFilePath)
                    ? await GetTopLevel(this).StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
                    : await GetTopLevel(this).StorageProvider.TryGetFolderFromPathAsync(document.FullFilePath),
                AllowMultiple = false,
            };

            var folders = await GetTopLevel(this).StorageProvider.OpenFolderPickerAsync(options);
            if (folders.Count > 0)
            {
                IStorageFolder folder = folders[0];
                if (folder != null)
                {
                    SavePath = folder.Path.LocalPath;
                    return SavePath;
                }
            }
        }
        else
        {
            var options = new FilePickerSaveOptions
            {
                Title = new LocalizedString("EXPORT_SAVE_TITLE"),
                SuggestedFileName = SuggestedName,
                SuggestedStartLocation = string.IsNullOrEmpty(document.FullFilePath)
                    ? await GetTopLevel(this).StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
                    : await GetTopLevel(this).StorageProvider.TryGetFolderFromPathAsync(document.FullFilePath),
                FileTypeChoices =
                    SupportedFilesHelper.BuildSaveFilter(SelectedExportIndex == 1
                        ? FileTypeDialogDataSet.SetKind.Video
                        : FileTypeDialogDataSet.SetKind.Image | FileTypeDialogDataSet.SetKind.Vector),
                ShowOverwritePrompt = true
            };

            var pickerResult = await GetTopLevel(this).StorageProvider.SaveFilePickerWithResultAsync(options);
            
            if (pickerResult.File is not { } file || string.IsNullOrEmpty(file.Name))
                return null;
            
            (SaveFormat, var fileName) = SupportedFilesHelper.GetSaveFileTypeAndPath(FileTypeDialogDataSet.SetKind.Any, pickerResult.File, pickerResult.SelectedFileType);
            
            return SaveFormat == null ? null : fileName;
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

    private static void RerenderPreview(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is ExportFilePopup popup)
        {
            if (popup.videoPreviewTimer != null)
            {
                popup.RenderPreview();
            }
        }
    }

    private static void UpdateOutput(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is ExportFilePopup popup)
        {
            popup.SaveWidth = popup.ExportOutput.OriginalSize.X;
            popup.SaveHeight = popup.ExportOutput.OriginalSize.Y;
            popup.RenderPreview();
            popup.UpdateSizeHint();
        }
    }
}
