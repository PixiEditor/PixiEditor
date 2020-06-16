using PixiEditor.Views;

namespace PixiEditor.Models.Dialogs
{
    internal class ImportFileDialog : CustomDialog
    {
        private int _fileHeight;


        private string _filePath;
        private int _fileWidth;

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

        public override bool ShowDialog()
        {
            ImportFilePopup popup = new ImportFilePopup();
            popup.FilePath = FilePath;
            popup.ShowDialog();
            if (popup.DialogResult == true)
            {
                FileHeight = popup.ImportHeight;
                FileWidth = popup.ImportWidth;
                FilePath = popup.FilePath;
            }

            return (bool) popup.DialogResult;
        }
    }
}