using Microsoft.Win32;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views.UserControls.AvalonDockWindows
{
    /// <summary>
    /// Interaction logic for ReferenceLayerWindow.xaml
    /// </summary>
    public partial class ReferenceLayerWindow : UserControl
    {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register(nameof(Document), typeof(Document), typeof(ReferenceLayerWindow));

        public Document Document
        {
            get => (Document)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(ReferenceLayerWindow));

        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public static readonly DependencyProperty LayerOpacityProperty =
            DependencyProperty.Register(nameof(LayerOpacity), typeof(float), typeof(ReferenceLayerWindow), new PropertyMetadata(100f));

        public float LayerOpacity
        {
            get => (float)GetValue(LayerOpacityProperty);
            set => SetValue(LayerOpacityProperty, value);
        }

        public static readonly DependencyProperty HasDocumentProperty =
            DependencyProperty.Register(nameof(HasDocument), typeof(bool), typeof(ReferenceLayerWindow));

        public bool HasDocument
        {
            get => (bool)GetValue(HasDocumentProperty);
            set => SetValue(HasDocumentProperty, value);
        }

        public RelayCommand UpdateLayerCommand { get; set; }

        public RelayCommand OpenFilePickerCommand { get; set; }

        public ReferenceLayerWindow()
        {
            UpdateLayerCommand = new RelayCommand(UpdateLayer);
            OpenFilePickerCommand = new RelayCommand(OpenFilePicker);
            InitializeComponent();
        }

        private void UpdateLayer(object obj)
        {
            Document.ReferenceLayer.LayerBitmap = Importer.ImportImage(FilePath);
            Document.ReferenceLayer.Opacity = LayerOpacity;
        }

        private void OpenFilePicker(object obj)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "PNG Files|*.png|JPEG Files|*.jpg;*.jpeg",
                CheckFileExists = true
            };

            if ((bool)dialog.ShowDialog())
            {
                FilePath = dialog.FileName;
            }
        }
    }
}
