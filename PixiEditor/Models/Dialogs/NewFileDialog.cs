using System.Windows;
using PixiEditor.Views;

namespace PixiEditor.Models.Dialogs
{
    public class NewFileDialog : CustomDialog
    {
        private int height;

        private int width;

        public int Width
        {
            get => width;
            set
            {
                if (width != value)
                {
                    width = value;
                    RaisePropertyChanged("Width");
                }
            }
        }

        public int Height
        {
            get => height;
            set
            {
                if (height != value)
                {
                    height = value;
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