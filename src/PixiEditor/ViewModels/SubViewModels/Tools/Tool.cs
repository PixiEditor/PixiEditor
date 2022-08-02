using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools;

internal abstract class ToolViewModel : NotifyableObject
{
    public KeyCombination Shortcut { get; set; }

    public virtual string ToolName => GetType().Name.Replace("Tool", string.Empty).Replace("ViewModel", string.Empty);

    public virtual string DisplayName => ToolName.AddSpacesBeforeUppercaseLetters();

    public virtual string ImagePath => $"/Images/Tools/{ToolName}Image.png";

    public virtual BrushShape BrushShape => BrushShape.Square;

    public virtual bool HideHighlight { get; }

    public abstract string Tooltip { get; }

    private string actionDisplay = string.Empty;
    public string ActionDisplay
    {
        get => actionDisplay;
        set
        {
            actionDisplay = value;
            RaisePropertyChanged(nameof(ActionDisplay));
        }
    }

    private bool isActive;
    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            RaisePropertyChanged(nameof(IsActive));
        }
    }

    public Cursor Cursor { get; set; } = Cursors.Arrow;

    public Toolbar Toolbar { get; set; } = new EmptyToolbar();

    public virtual void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown, bool altIsDown) { }
    public virtual void OnLeftMouseButtonDown(VecD pos) { }
}
