using PixiEditor.Models.Enums;
using PixiEditor.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views
{
    public partial class ExportFilePopup : Window, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SaveHeightProperty =
            DependencyProperty.Register("SaveHeight", typeof(int), typeof(ExportFilePopup), new PropertyMetadata(32));


        public static readonly DependencyProperty SaveWidthProperty =
            DependencyProperty.Register("SaveWidth", typeof(int), typeof(ExportFilePopup), new PropertyMetadata(32));

        private readonly SaveFilePopupViewModel dataContext = new SaveFilePopupViewModel();

        public event PropertyChangedEventHandler PropertyChanged;

        private int imageWidth;
        private int imageHeight;
        public string SizeHint => $"If you want to share the image, try {GetBestPercentage()}% for the best clarity";

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
}
