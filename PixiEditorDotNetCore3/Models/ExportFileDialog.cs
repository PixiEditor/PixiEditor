using PixiEditor.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PixiEditor.Models
{
    public class ExportFileDialog : CustomDialog
    {

        private int _fileWidth;

        public int FileWidth
        {
            get { return _fileWidth; }
            set { if (_fileWidth != value) { _fileWidth = value; RaisePropertyChanged("Width"); } }
        }


        private int _fileHeight;

        public int FileHeight
        {
            get { return _fileHeight; }
            set { if (_fileHeight != value) { _fileHeight = value; RaisePropertyChanged("FileHeight"); } }
        }


        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }
            set { if (_filePath != value) { _filePath = value; RaisePropertyChanged("FilePath"); } }
        }

        public ExportFileDialog(Size fileDimensions)
        {
            FileHeight = (int)fileDimensions.Height;
            FileWidth = (int)fileDimensions.Width;
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
