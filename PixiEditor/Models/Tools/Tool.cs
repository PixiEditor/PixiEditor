using PixiEditor.Helpers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Models.Tools
{
    public abstract class Tool : NotifyableObject
    {
        public abstract BitmapPixelChanges Use(Layer layer, Coordinates[] pixels, Color color);
        public abstract ToolType ToolType { get; }
        public string ImagePath => $"/Images/{ToolType}Image.png";
        public bool PerformsOperationOnBitmap { get; set; } = true;
        public bool HideHighlight { get; set; } = false;
        public bool RequiresPreviewLayer { get; set; }
        public string Tooltip { get; set; }

        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set 
            { 
                _isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }

        public Cursor Cursor { get; set; } = Cursors.Arrow;
        public Toolbar Toolbar { get; set; } = new EmptyToolbar();
    }
}
