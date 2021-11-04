using PixiEditor.Models.Enums;
using PixiEditor.Views;

namespace PixiEditor.Models.Dialogs
{
    public class ResizeDocumentDialog : CustomDialog
    {
        private int height;
        private int width;

        public ResizeDocumentDialog(int currentWidth, int currentHeight, bool openResizeCanvas = false)
        {
            // Remove this
            throw new System.Exception("Just doing some testing stuff");

            Width = currentWidth;
            Height = currentHeight;
            OpenResizeCanvas = openResizeCanvas;
        }

        public bool OpenResizeCanvas { get; set; }

        public AnchorPoint ResizeAnchor { get; set; }

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
            return OpenResizeCanvas ? ShowResizeCanvasDialog() : ShowResizeDocumentCanvas();
        }

        private bool ShowResizeDocumentCanvas()
        {
            ResizeDocumentPopup popup = new ResizeDocumentPopup
            {
                NewHeight = Height,
                NewWidth = Width
            };

            popup.ShowDialog();
            if (popup.DialogResult == true)
            {
                Width = popup.NewWidth;
                Height = popup.NewHeight;
            }

            return (bool)popup.DialogResult;
        }

        private bool ShowResizeCanvasDialog()
        {
            ResizeCanvasPopup popup = new ResizeCanvasPopup
            {
                NewHeight = Height,
                NewWidth = Width
            };

            popup.ShowDialog();
            if (popup.DialogResult == true)
            {
                Width = popup.NewWidth;
                Height = popup.NewHeight;
                ResizeAnchor = popup.SelectedAnchorPoint;
            }

            return (bool)popup.DialogResult;
        }
    }
}