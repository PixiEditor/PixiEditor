using System.Windows;
using PixiEditor.Views;

namespace PixiEditor.Models.Dialogs
{
    public class NewFileDialog : CustomDialog
    {
        private int _height;

        private int _width;

        public int Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    RaisePropertyChanged("Width");
                }
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    RaisePropertyChanged("Height");
                }
            }
        }

        public override bool ShowDialog()
        {
            Window popup = new NewFilePopup();
            popup.ShowDialog();
            Height = (popup as NewFilePopup).FileHeight;
            Width = (popup as NewFilePopup).FileWidth;
            return (bool) popup.DialogResult;
        }
    }
}