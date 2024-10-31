using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Input;
using Drawie.Numerics;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools;

internal abstract class ToolViewModel : ObservableObject, IToolHandler
{
    private bool canBeUsedOnActiveLayerOnActiveLayer = true;
    public bool IsTransient { get; set; } = false;
    public KeyCombination Shortcut { get; set; }

    public virtual string ToolName => GetType().Name.Replace("Tool", string.Empty).Replace("ViewModel", string.Empty);

    public abstract string ToolNameLocalizationKey { get; }
    public virtual LocalizedString DisplayName => new LocalizedString(ToolNameLocalizationKey);

    public virtual string Icon => $"\u25a1";

    public virtual BrushShape BrushShape => BrushShape.Square;

    public abstract Type[]? SupportedLayerTypes { get; }

    public bool CanBeUsedOnActiveLayer
    {
        get => canBeUsedOnActiveLayerOnActiveLayer;
        private set
        {
            canBeUsedOnActiveLayerOnActiveLayer = value;
            OnPropertyChanged(nameof(CanBeUsedOnActiveLayer));
        }
    }

    public abstract Type LayerTypeToCreateOnEmptyUse { get; }

    public virtual bool HideHighlight { get; }

    public abstract LocalizedString Tooltip { get; }

    /// <summary>
    /// Determines if secondary color should be used if right click mode is set to secondary color
    /// </summary>
    public virtual bool UsesColor => false;

    /// <summary>
    /// Determines if PixiEditor should switch to the Eraser when right click mode is set to erase
    /// </summary>
    public virtual bool IsErasable => false;

    /// <inheritdoc cref="IToolHandler.StopsLinkedToolOnUse"/>
    public virtual bool StopsLinkedToolOnUse => true;

    /// <summary>
    /// The mouse button that is being used with the tool
    /// </summary>
    public MouseButton UsedWith { get; set; }

    private LocalizedString actionDisplay = string.Empty;

    public LocalizedString ActionDisplay
    {
        get => actionDisplay;
        set
        {
            actionDisplay = value;
            OnPropertyChanged(nameof(ActionDisplay));
        }
    }

    private bool isActive;

    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            OnPropertyChanged(nameof(IsActive));
        }
    }

    public Cursor Cursor { get; set; } = new Cursor(StandardCursorType.Arrow);

    public IToolbar Toolbar { get; set; } = new EmptyToolbar();

    internal ToolViewModel()
    {
        ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
    }

    internal void SelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        if (layers.Length is > 1 or 0)
        {
            CanBeUsedOnActiveLayer = SupportedLayerTypes == null;
            return;
        }
        
        var layer = layers[0];

        if (IsActive)
        {
            OnSelectedLayersChanged(layers);
        }

        if (SupportedLayerTypes == null)
        {
            CanBeUsedOnActiveLayer = true;
            return;
        }

        foreach (var type in SupportedLayerTypes)
        {
            if (type.IsInstanceOfType(layer) || IsFolderAndRasterSupported(layer))
            {
                CanBeUsedOnActiveLayer = true;
                return;
            }
        }

        CanBeUsedOnActiveLayer = false;
    }
    
    private bool IsFolderAndRasterSupported(IStructureMemberHandler layer)
    {
        return SupportedLayerTypes.Contains(typeof(IRasterLayerHandler)) && layer is IFolderHandler;
    }

    private void OnLanguageChanged(Language obj)
    {
        ActionDisplay = new LocalizedString(ActionDisplay.Key);
    }

    public virtual void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown) { }

    public virtual void UseTool(VecD pos) { }
    public virtual void OnSelected() { }
    
    protected virtual void OnSelectedLayersChanged(IStructureMemberHandler[] layers) { }

    public virtual void OnDeselecting()
    {
    }

    protected T GetValue<T>([CallerMemberName] string name = null)
    {
        var setting = Toolbar.GetSetting(name);

        if (setting.GetSettingType().IsAssignableTo(typeof(Enum)))
        {
            var property = setting.GetType().GetProperty("Value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            return (T)property!.GetValue(setting);
        }

        return (T)setting.Value;
    }
}
