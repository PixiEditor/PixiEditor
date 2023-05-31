using System.Windows;
using System.Windows.Input;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Localization;
using PixiEditor.ViewModels;

namespace PixiEditor.Views.Dialogs;

internal partial class ExportFilePopup : Window
{
    public static readonly DependencyProperty SaveHeightProperty =
        DependencyProperty.Register(nameof(SaveHeight), typeof(int), typeof(ExportFilePopup), new PropertyMetadata(32));


    public static readonly DependencyProperty SaveWidthProperty =
        DependencyProperty.Register(nameof(SaveWidth), typeof(int), typeof(ExportFilePopup), new PropertyMetadata(32));

    public static readonly DependencyProperty SetBestPercentageCommandProperty =
        DependencyProperty.Register(nameof(SetBestPercentageCommand), typeof(RelayCommand), typeof(ExportFilePopup));

    private readonly SaveFilePopupViewModel dataContext = new SaveFilePopupViewModel();

    public RelayCommand SetBestPercentageCommand
    {
        get => (RelayCommand)GetValue(SetBestPercentageCommandProperty);
        set => SetValue(SetBestPercentageCommandProperty, value);
    }

    private int imageWidth;
    private int imageHeight;
    public string SizeHint => new LocalizedString("EXPORT_SIZE_HINT", GetBestPercentage());

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    public ExportFilePopup(int imageWidth, int imageHeight)
    {
        this.imageWidth = imageWidth;
        this.imageHeight = imageHeight;

        InitializeComponent();
        Owner = Application.Current.MainWindow;
        DataContext = dataContext;
        Loaded += (_, _) => sizePicker.FocusWidthPicker();

        SaveWidth = imageWidth;
        SaveHeight = imageHeight;

        SetBestPercentageCommand = new RelayCommand(SetBestPercentage);
    }

    private int GetBestPercentage()
    {
        int maxDim = Math.Max(imageWidth, imageWidth);
        for (int i = 16; i >= 1; i--)
        {
            if (maxDim * i <= 1280)
                return i * 100;
        }
        return 100;
    }

    private void SetBestPercentage(object parameter)
    {
        sizePicker.ChosenPercentageSize = GetBestPercentage();
        sizePicker.PercentageRb.IsChecked = true;
        sizePicker.PercentageLostFocus(null);
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

    public string SavePath
    {
        get => dataContext.FilePath;
        set => dataContext.FilePath = value;
    }

    public FileType SaveFormat
    {
        get => dataContext.ChosenFormat;
        set => dataContext.ChosenFormat = value;
    }
}
