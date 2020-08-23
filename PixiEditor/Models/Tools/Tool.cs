using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Tools.ToolSettings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools
{
    public abstract class Tool : NotifyableObject
    {
        public abstract ToolType ToolType { get; }
        public string ImagePath => $"/Images/{ToolType}Image.png";
        public bool HideHighlight { get; set; } = false;
        public string Tooltip { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }

        public Cursor Cursor { get; set; } = Cursors.Arrow;

        public Toolbar Toolbar { get; set; } = new EmptyToolbar();

        private bool _isActive;
        public bool CanStartOutsideCanvas { get; set; } = false;

        public virtual void OnMouseDown(MouseEventArgs e) { }

        public virtual void OnMouseUp(MouseEventArgs e) { }

        public virtual void OnMouseMove(MouseEventArgs e) { }

        public virtual void AfterAddedUndo() { }
    }
}