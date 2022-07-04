using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Tools.ToolSettings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using System.Windows.Input;
using SkiaSharp;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Tools
{
    public abstract class Tool : NotifyableObject
    {
        public KeyCombination Shortcut { get; set; }

        public virtual string ToolName => GetType().Name.Replace("Tool", string.Empty);

        public virtual string DisplayName => ToolName.AddSpacesBeforeUppercaseLetters();

        public virtual string ImagePath => $"/Images/Tools/{ToolName}Image.png";

        public virtual bool HideHighlight { get; }

        public virtual bool RequiresPreciseMouseData { get; }

        public abstract string Tooltip { get; }

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

        public ToolSession Session { get; set; }

        private bool isActive;
        private string actionDisplay = string.Empty;

        public virtual void OnKeyDown(Key key) { }

        public virtual void OnKeyUp(Key key) { }

        public virtual void BeforeUse() { }

        /// <summary>
        ///     Called when the tool finished executing
        /// </summary>
        /// <param name="sessionRect">A rectangle which was created during session</param>
        public virtual void AfterUse(SKRectI sessionRect) { }

        public virtual void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown) { }
    }
}
