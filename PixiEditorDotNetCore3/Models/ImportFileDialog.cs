using PixiEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PixiEditor.Models
{
    class ImportFileDialog : CustomDialog
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

        public override bool ShowDialog()
        {
            ImportFilePopup popup = new ImportFilePopup();
            popup.ShowDialog();
            if (popup.DialogResult == true)
            {
                FileHeight = popup.ImportHeight;
                FileWidth = popup.ImportWidth;
                FilePath = popup.FilePath;
            }
            return (bool)popup.DialogResult;
        }
    }
}
