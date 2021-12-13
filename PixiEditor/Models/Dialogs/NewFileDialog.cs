using PixiEditor.Models.UserPreferences;
using PixiEditor.Views;

namespace PixiEditor.Models.Dialogs
{
    public class NewFileDialog : CustomDialog
    {
        public const int defaultSize = 64;

        private int height = IPreferences.Current.GetPreference("DefaultNewFileHeight", defaultSize);

        private int width = IPreferences.Current.GetPreference("DefaultNewFileWidth", defaultSize);

        public int Width
        {
            get => width;
            set => SetProperty(ref width, value);
        }

        public int Height
        {
            get => height;
            set => SetProperty(ref height, value);
        }

        public override bool ShowDialog()
        {
            NewFilePopup popup = new()
            {
                FileWidth = Width,
                FileHeight = Height
            };

            popup.ShowDialog();

            Height = popup.FileHeight;
            Width = popup.FileWidth;

            return popup.DialogResult.GetValueOrDefault();
        }
    }
}