using System;
using System.Windows;
using PixiEditor.Models.UserPreferences;
using PixiEditor.Views;

namespace PixiEditor.Models.Dialogs
{
    public class NewFileDialog : CustomDialog
    {
        private int height = (int)IPreferences.Current.GetPreference("DefaultNewFileHeight", 16L);

        private int width = (int)IPreferences.Current.GetPreference("DefaultNewFileWidth", 16L);

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
            Window popup = new NewFilePopup()
            {
                FileWidth = Width,
                FileHeight = Height
            };
            popup.ShowDialog();
            Height = (popup as NewFilePopup).FileHeight;
            Width = (popup as NewFilePopup).FileWidth;
            return (bool)popup.DialogResult;
        }
    }
}