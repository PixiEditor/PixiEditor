using System.Windows;
using PixiEditor.Views;

namespace PixiEditor.Models.Dialogs
{
    public class ExportFileDialog : CustomDialog
    {
        public int FileWidth
        {
            get => _fileWidth;
            set
            {
                if (_fileWidth != value)
                {
                    _fileWidth = value;
                    RaisePropertyChanged("Width");
                }
            }
        }

        public int FileHeight
        {
            get => _fileHeight;
            set
            {
                if (_fileHeight != value)
                {
                    _fileHeight = value;
                    RaisePropertyChanged("FileHeight");
                }
            }
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    RaisePropertyChanged("FilePath");
                }
            }
        }

        private int _fileHeight;


        private string _filePath;

        private int _fileWidth;

        public ExportFileDialog(Size fileDimensions)
        {
            FileHeight = (int) fileDimensions.Height;
            FileWidth = (int) fileDimensions.Width;
        }

        public override bool ShowDialog()
        {
            SaveFilePopup popup = new SaveFilePopup
            {
                SaveWidth = FileWidth,
                SaveHeight = FileHeight
            };
            popup.ShowDialog();
            if (popup.DialogResult == true)
            {
                FileWidth = popup.SaveWidth;
                FileHeight = popup.SaveHeight;
                FilePath = popup.SavePath;
            }

            return (bool) popup.DialogResult;
        }
    }
}