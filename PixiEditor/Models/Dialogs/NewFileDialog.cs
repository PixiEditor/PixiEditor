using PixiEditor.Views;
using System.Windows;

namespace PixiEditor.Models.Dialogs
{
    public class NewFileDialog : CustomDialog
    {

        private int _width;

        public int Width
        {
            get { return _width; }
            set { if (_width != value) { _width = value; RaisePropertyChanged("Width"); } }
        }


        private int _height;

        public int Height
        {
            get { return _height; }
            set { if (_height != value) { _height = value; RaisePropertyChanged("Height"); } }
        }

        public override bool ShowDialog()
        {
            Window popup = new NewFilePopup();
            popup.ShowDialog();
            Height = (popup as NewFilePopup).FileHeight;
            Width = (popup as NewFilePopup).FileWidth;
            return (bool)popup.DialogResult;
        }
    }
}
