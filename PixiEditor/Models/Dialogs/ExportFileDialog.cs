using System.Windows;
using PixiEditor.Views;

namespace PixiEditor.Models.Dialogs
{
    public class ExportFileDialog : CustomDialog
    {
        private int fileHeight;

        private string filePath;

        private int fileWidth;

        public ExportFileDialog(Size fileDimensions)
        {
            FileHeight = (int)fileDimensions.Height;
            FileWidth = (int)fileDimensions.Width;
        }

        public int FileWidth
        {
            get => fileWidth;
            set
            {
                if (fileWidth != value)
                {
                    fileWidth = value;
                    RaisePropertyChanged("Width");
                }
            }
        }

        public int FileHeight
        {
            get => fileHeight;
            set
            {
                if (fileHeight != value)
                {
                    fileHeight = value;
                    RaisePropertyChanged("FileHeight");
                }
            }
        }

        public string FilePath
        {
            get => filePath;
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    RaisePropertyChanged("FilePath");
                }
            }
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

            return (bool)popup.DialogResult;
        }
    }
}