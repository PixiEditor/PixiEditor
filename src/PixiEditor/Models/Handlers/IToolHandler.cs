using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers.Toolbars;
using Drawie.Numerics;
using PixiEditor.Models.Input;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Handlers;

internal interface IToolHandler : IHandler
{
    public bool IsTransient { get; set; }
    public LocalizedString DisplayName => new LocalizedString(ToolNameLocalizationKey);
    public string ToolName => GetType().Name.Replace("Tool", string.Empty).Replace("ViewModel", string.Empty);
    public string ToolNameLocalizationKey { get; }
    public string DefaultIcon => $"icon-{ToolName.ToLower()}";
    public Type[]? SupportedLayerTypes { get; }

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
    public bool CanBeUsedOnActiveLayer { get; }
    
    /// <summary>
    ///     Layer type that should be created if no layer is selected incompatible one.
    /// </summary>
    public Type? LayerTypeToCreateOnEmptyUse { get; }

    public virtual string? DefaultNewLayerName => null;

    public void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey);
    public void UseTool(VecD pos);
    public void OnToolSelected(bool restoring);

    public void SetToolSetSettings(IToolSetHandler toolset, Dictionary<string, object>? settings);
    public void ApplyToolSetSettings(IToolSetHandler toolset);
    public void OnToolDeselected(bool transient);
    public void OnPostUndoInlet();
    public void OnPostRedoInlet();
    public void OnActiveFrameChanged(int newFrame);
    public void OnPreUndoInlet();
    public void OnPreRedoInlet();
    public void QuickToolSwitchInlet();
}
