using PixiEditor.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditor.Models.Dialogs
{
    public class ResizeDocumentDialog : CustomDialog
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

        public ResizeDocumentDialog(int currentWidth, int currentHeight)
        {
            Width = currentWidth;
            Height = currentHeight;
        }

        public override bool ShowDialog()
        {
            ResizeDocumentPopup popup = new ResizeDocumentPopup(); 
            ResizeDocumentPopup result = popup.DataContext as ResizeDocumentPopup;
            result.NewHeight = Height;
            result.NewWidth = Width;

            popup.ShowDialog();
            if (popup.DialogResult == true)
            {
                Width = result.NewWidth;
                Height = result.NewHeight;
            }
            return (bool)popup.DialogResult;
        }
    }
}
