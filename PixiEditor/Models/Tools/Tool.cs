using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Tools.ToolSettings;

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

        public virtual void OnMouseDown()
        {
        }

        public virtual void OnMouseUp()
        {
        }

        public virtual void AfterAddedUndo()
        {
        }
    }
}