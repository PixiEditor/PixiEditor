using System;
using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Tools.ToolSettings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.Tools
{
    public abstract class Tool : NotifyableObject
    {
        protected string name;
        private bool isActive;
        private string actionDisplay = string.Empty;

        public string ToolName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return GetType().Name.Replace("Tool", string.Empty);
                }

                return name;
            }
        }

        public string ImagePath => $"/Images/{ToolName}Image.png";

        public bool HideHighlight { get; set; } = false;

        public string Tooltip { get; set; }

        public string ActionDisplay
        {
            get => actionDisplay;
            set
            {
                actionDisplay = value;
                RaisePropertyChanged("ActionDisplay");
            }
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }

        public Cursor Cursor { get; set; } = Cursors.Arrow;

        public Toolbar Toolbar { get; set; } = new EmptyToolbar();

        public bool CanStartOutsideCanvas { get; set; } = false;

        public virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        public virtual void OnKeyDown(KeyEventArgs e)
        {
        }

        public virtual void OnKeyUp(KeyEventArgs e)
        {
        }

        public virtual void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
        }

        public virtual void OnStoppedRecordingMouseUp(MouseEventArgs e)
        {
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
        }

        public virtual void AfterAddedUndo(UndoManager undoManager)
        {
        }
    }
}