using PixiEditor.Helpers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Models.Tools
{
    public abstract class Tool : NotifyableObject
    {
        public abstract BitmapPixelChanges Use(Layer layer, Coordinates[] pixels, Color color, int toolSize);
        public abstract ToolType ToolType { get; }
        public string ImagePath => $"/Images/{ToolType.ToString()}Image.png";
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
    }
}
