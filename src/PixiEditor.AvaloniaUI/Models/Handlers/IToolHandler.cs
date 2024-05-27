using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Handlers.Toolbars;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface IToolHandler : IHandler
{
    public bool IsTransient { get; set; }
    public LocalizedString DisplayName => new LocalizedString(ToolNameLocalizationKey);
    public string ToolName => GetType().Name.Replace("Tool", string.Empty).Replace("ViewModel", string.Empty);
    public string ToolNameLocalizationKey { get; }
    public string IconKey => $"icon-{ToolName.ToLower()}";
    //public virtual BrushShape BrushShape => BrushShape.Square;

    public bool HideHighlight { get; }

    public IToolbar Toolbar { get; set; }

    public abstract LocalizedString Tooltip { get; }

    /// <summary>
    /// Determines if secondary color should be used if right click mode is set to secondary color
    /// </summary>
    public virtual bool UsesColor => false;

    /// <summary>
    /// Determines if PixiEditor should switch to the Eraser when right click mode is set to erase
    /// </summary>
    public virtual bool IsErasable => false;

    /// <summary>
    /// Indicates whether active linked tool stops on use.
    /// </summary>
    /// <remarks>
    /// If this property is true, the linked tool will stop executing when used.
    /// If this property is false, the linked tool will continue executing even after being used.
    /// </remarks>
    public virtual bool StopsLinkedToolOnUse => true;

    /// <summary>
    /// The mouse button that is being used with the tool
    /// </summary>
    public MouseButton UsedWith { get; set; }
    public LocalizedString ActionDisplay { get; set; }
    public bool IsActive { get; set; }
    public Cursor Cursor { get; set; }
    //public Toolbar Toolbar { get; set; }

    public void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown);
    public void UseTool(VecD pos);
    public void OnSelected();

    public void OnDeselecting();
}
