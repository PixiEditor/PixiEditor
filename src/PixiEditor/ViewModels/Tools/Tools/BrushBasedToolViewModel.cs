using Avalonia.Input;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Config;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Input;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.BrushSystem;
using PixiEditor.ViewModels.Document.Blackboard;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Tools.Tools;

internal class BrushBasedToolViewModel : ToolViewModel, IBrushToolHandler
{
    private VecD lastPoint;

    private List<Setting> brushShapeSettings = new();
    public override Type[]? SupportedLayerTypes { get; } = { typeof(IRasterLayerHandler) };
    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

    public override LocalizedString Tooltip => new LocalizedString(toolTipKey, Shortcut);
    public override string ToolNameLocalizationKey => toolName;
    public override string ToolName => toolName ?? base.ToolName;
    public bool IsCustomBrushTool { get; private set; }
    public override bool UsesColor => true;
    public override bool IsErasable => true;
    public KeyCombination? DefaultShortcut { get; set; }
    public bool SupportsSecondaryActionOnRightClick { get; set; }

    public VecD LastAppliedPoint
    {
        get => lastPoint;
        set => SetProperty(ref lastPoint, value);
    }

    [Settings.Inherited] public double ToolSize => GetValue<double>();

    private string? toolName;
    private string toolTipKey;

    private List<ParsedActionDisplayConfig>? actionDisplays;

    public BrushBasedToolViewModel()
    {
        Cursor = Cursors.PreciseCursor;
        Toolbar = CreateToolbar();

        (Toolbar as Toolbar).SettingChanged += OnSettingChanged;
    }

    public BrushBasedToolViewModel(BrushViewModel brush, string? tooltip, string? toolName, KeyCombination? defaultShortcut,
        List<ActionDisplayConfig>? actionDisplays, bool supportsSecondaryActionOnRightClick)
    {
        Cursor = Cursors.PreciseCursor;
        Toolbar = CreateToolbar();

        (Toolbar as Toolbar).SettingChanged += OnSettingChanged;

        var brushSetting = Toolbar.GetSetting(nameof(BrushToolbar.Brush));

        brushSetting.Value = brush;
        brushSetting.IsExposed = false;

        this.toolName = toolName ?? brush.Name;
        toolTipKey = tooltip ?? toolName ?? brush.Name;
        DefaultShortcut = defaultShortcut;
        IsCustomBrushTool = true;
        this.actionDisplays = ParseActionDisplays(actionDisplays);
        SupportsSecondaryActionOnRightClick = supportsSecondaryActionOnRightClick;

        if (this.actionDisplays is { Count: > 0 })
        {
            var defaultDisplay = this.actionDisplays.FirstOrDefault(x => x.Modifiers == KeyModifiers.None);
            if (defaultDisplay != null)
            {
                ActionDisplay = defaultDisplay.ActionDisplay;
            }
        }
    }

    protected virtual Toolbar CreateToolbar()
    {
        return ToolbarFactory.Create<BrushBasedToolViewModel, BrushToolbar>(this);
    }

    protected virtual void SwitchToTool()
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseBrushBasedTool(this);
    }

    public override void UseTool(VecD pos)
    {
        SwitchToTool();
    }

    public void OnToolSelected(bool restoring)
    {
        SwitchToTool();
    }

    public void OnPostUndoInlet()
    {
        SwitchToTool();
    }

    public override void OnPostRedoInlet()
    {
        SwitchToTool();
    }

    public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey)
    {
        if (actionDisplays is null || actionDisplays.Count == 0)
        {
            return;
        }

        var modifiers = KeyModifiers.None;
        if (ctrlIsDown)
        {
            modifiers |= KeyModifiers.Control;
        }

        if (shiftIsDown)
        {
            modifiers |= KeyModifiers.Shift;
        }

        if (altIsDown)
        {
            modifiers |= KeyModifiers.Alt;
        }

        var display = actionDisplays.FirstOrDefault(x => x.Modifiers == modifiers);
        if (display != null)
        {
            ActionDisplay = display.ActionDisplay;
        }
    }

    private void OnSettingChanged(string name, object value)
    {
        if (value is BrushViewModel)
        {
            AddBrushShapeSettings();
        }
    }

    protected override void OnSelectedLayersChanged(IStructureMemberHandler[] layers)
    {
        OnDeselecting(false);
        OnToolSelected(false);
    }

    private void AddBrushShapeSettings()
    {
        foreach (var setting in brushShapeSettings)
        {
            Toolbar.RemoveSetting(setting);
        }

        brushShapeSettings.Clear();

        var blackboard = ((BrushToolbar)Toolbar).Brush?.Document?.NodeGraphHandler?.Blackboard;
        if (blackboard is null)
            return;

        foreach (var blackboardVariable in blackboard.Variables)
        {
            if (blackboardVariable is VariableViewModel { IsExposedBindable: true } varVm)
            {
                Toolbar.AddSetting(varVm.SettingView);
                brushShapeSettings.Add(varVm.SettingView);
            }
        }
    }

    protected override void OnDeselecting(bool transient)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.TryStopActiveTool();
    }

    private List<ParsedActionDisplayConfig>? ParseActionDisplays(List<ActionDisplayConfig>? configs)
    {
        if (configs is null || configs.Count == 0)
        {
            return null;
        }

        List<ParsedActionDisplayConfig> parsed = new();

        foreach (var config in configs)
        {
            var combination = KeyCombination.TryParse(config.Modifiers);
            parsed.Add(new ParsedActionDisplayConfig(combination.Modifiers, config.ActionDisplay));
        }

        return parsed;
    }
}

record ParsedActionDisplayConfig(KeyModifiers Modifiers, string ActionDisplay);
