using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Input;
using Drawie.Numerics;
using PixiEditor.Helpers;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools;

internal abstract class ToolViewModel : ObservableObject, IToolHandler
{
    private bool canBeUsedOnActiveLayerOnActiveLayer = true;
    private VectorPath? brushShape;
    public bool IsTransient { get; set; } = false;
    public KeyCombination Shortcut { get; set; }

    public virtual string ToolName => GetType().Name.Replace("Tool", string.Empty).Replace("ViewModel", string.Empty);

    public abstract string ToolNameLocalizationKey { get; }
    public virtual LocalizedString DisplayName => new LocalizedString(ToolNameLocalizationKey);

    public virtual string DefaultIcon => PixiPerfectIcons.Placeholder;

    public VectorPath? FinalBrushShape
    {
        get => brushShape;
        set => SetProperty(ref brushShape, value);
    }

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

    public string IconOverwrite { get; set; }

    public string IconToUse => IconOverwrite ?? DefaultIcon;

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

    public Dictionary<IToolSetHandler, Dictionary<string, object>> ToolSetSettings { get; } = new();
    public bool IsPixiPerfectIcon => !Uri.TryCreate(IconToUse, UriKind.Absolute, out _);

    internal ToolViewModel()
    {
        ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
    }

    internal void SelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        if (layers.Length is > 1 or 0)
        {
            CanBeUsedOnActiveLayer = SupportedLayerTypes == null;
            if (IsActive)
            {
                OnSelectedLayersChanged(layers);
            }

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
            if (type.IsInstanceOfType(layer) || IsMaskSelectedAndRasterSupported(layer))
            {
                CanBeUsedOnActiveLayer = true;
                return;
            }
        }

        CanBeUsedOnActiveLayer = false;
    }

    private bool IsMaskSelectedAndRasterSupported(IStructureMemberHandler layer)
    {
        return SupportedLayerTypes.Contains(typeof(IRasterLayerHandler)) && layer is IFolderHandler or ILayerHandler
        {
            ShouldDrawOnMask: true
        };
    }

    private void OnLanguageChanged(Language obj)
    {
        ActionDisplay = new LocalizedString(ActionDisplay.Key);
    }

    public virtual void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey) { }

    public virtual void UseTool(VecD pos) { }

    protected virtual void OnSelected(bool restoring) { }

    public void OnToolSelected(bool restoring)
    {
        if (!restoring)
        {
            IsActive = true;
        }

        OnSelected(restoring);
    }

    protected virtual void OnSelectedLayersChanged(IStructureMemberHandler[] layers) { }

    public void OnToolDeselected(bool transient)
    {
        if (!transient)
        {
            IsActive = false;
        }

        OnDeselecting(transient);
    }

    protected virtual void OnDeselecting(bool transient)
    {
    }

    public virtual void OnPostUndoInlet() { }
    public virtual void OnPostRedoInlet() { }
    public virtual void OnActiveFrameChanged(int newFrame) { }
    public virtual void OnPreUndoInlet() { }

    public virtual void OnPreRedoInlet() { }

    public virtual void QuickToolSwitchInlet()
    {
    }

    public void SetToolSetSettings(IToolSetHandler toolset, Dictionary<string, object>? settings)
    {
        if (settings == null || settings.Count == 0 || toolset == null)
        {
            return;
        }

        foreach (var valueSetting in settings)
        {
            if (valueSetting.Value is long)
            {
                settings[valueSetting.Key] = Convert.ToSingle(valueSetting.Value);
            }
        }

        ToolSetSettings[toolset] = settings;
    }

    public void ApplyToolSetSettings(IToolSetHandler toolset)
    {
        IconOverwrite = null;
        var toolbarSettings = Toolbar.Settings.ToArray();
        foreach (var toolbarSetting in toolbarSettings)
        {
            toolbarSetting.ResetOverwrite();
        }

        if (toolset.IconOverwrites.TryGetValue(this, out var icon))
        {
            IconOverwrite = icon;
        }

        if (ToolSetSettings.TryGetValue(toolset, out var settings))
        {
            foreach (var setting in settings)
            {
                if (IsDefaultSetting(setting, out object defaultValue))
                {
                    string settingName = setting.Key.Replace("Default", string.Empty);
                    var foundSetting = TryGetSettingByName(settingName, setting);
                    if (foundSetting is null)
                    {
                        continue;
                    }

                    if (defaultValue is JsonElement jsonElement)
                    {
                        try
                        {
                            defaultValue = JsonUtility.TryDeserialize(jsonElement, foundSetting.GetSettingType());
                        }
                        catch (JsonException)
                        {
#if DEBUG
                            Debug.WriteLine(
                                $"Failed to deserialize default value for setting {settingName} in toolset {toolset.Name}");
#endif
                        }

                        foundSetting.SetDefaultValue(defaultValue, toolset.Name);
                    }
                }
            }

            foreach (var toolbarSetting in toolbarSettings)
            {
                toolbarSetting.SetCurrentToolset(toolset.Name);
            }

            if (settings is null)
            {
                return;
            }

            foreach (var setting in settings)
            {
                if (IsExposeSetting(setting, out bool expose))
                {
                    string settingName = setting.Key.Replace("Expose", string.Empty);
                    var foundSetting = TryGetSettingByName(settingName, setting);
                    if (foundSetting is null)
                    {
                        continue;
                    }

                    foundSetting.SetOverwriteExposed(expose);
                }
                else if (IsDefaultSetting(setting, out object defaultValue))
                {
                    continue;
                }
                else
                {
                    try
                    {
                        var foundSetting = TryGetSettingByName(setting.Key, setting);
                        if (foundSetting is null)
                        {
                            continue;
                        }

                        foundSetting.SetOverwriteValue(setting.Value);
                    }
                    catch (InvalidCastException)
                    {
#if DEBUG
                        throw;
#endif
                    }
                }
            }
        }
    }

    private Setting? TryGetSettingByName(string settingName, KeyValuePair<string, object> setting)
    {
        var foundSetting = Toolbar.GetSetting(settingName);
        if (foundSetting is null)
        {
#if DEBUG
            Debug.WriteLine($"Setting {settingName} not found in toolbar {Toolbar.GetType().Name}");
#endif
        }

        return foundSetting;
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

        if (setting.Value is JsonElement json)
        {
            return json.Deserialize<T>();
        }

        try
        {
            return (T)setting.Value;
        }
        catch (InvalidCastException)
        {
            if (typeof(T) == typeof(float) || typeof(T) == typeof(double) || typeof(T) == typeof(int))
            {
                return (T)(object)Convert.ToSingle(setting.Value);
            }

            throw;
        }
    }

    protected void SetValue<T>(T value, [CallerMemberName] string name = null)
    {
        var setting = Toolbar.GetSetting(name);
        if (setting is null)
        {
            throw new InvalidOperationException($"Setting {name} not found in toolbar {Toolbar.GetType().Name}");
        }

        if (setting.GetSettingType() != typeof(T))
        {
            throw new InvalidCastException($"Setting {name} is not of type {typeof(T).Name}");
        }

        setting.Value = value;
    }


    private bool IsExposeSetting(KeyValuePair<string, object> settingConfig, out bool expose)
    {
        bool isExpose = settingConfig.Key.StartsWith("Expose", StringComparison.InvariantCultureIgnoreCase);
        if (!isExpose)
        {
            expose = false;
            return false;
        }

        var settingName = settingConfig.Key.Replace("Expose", string.Empty);

        if (settingConfig.Value is bool value)
        {
            expose = value;
            return true;
        }

        expose = false;
        return false;
    }

    private bool IsDefaultSetting(KeyValuePair<string, object> settingConfig, out object defaultValue)
    {
        bool isDefault = settingConfig.Key.StartsWith("Default", StringComparison.InvariantCultureIgnoreCase);
        if (!isDefault)
        {
            defaultValue = null!;
            return false;
        }

        var settingName = settingConfig.Key.Replace("Default", string.Empty);
        defaultValue = settingConfig.Value;
        return true;
    }
}
