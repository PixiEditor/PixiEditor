using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Input;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document.Blackboard;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Tools.Tools;

internal class BrushBasedToolViewModel : ToolViewModel, IBrushToolHandler
{
    private List<Setting> brushShapeSettings = new();
    public override Type[]? SupportedLayerTypes { get; } = { typeof(IRasterLayerHandler) };
    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

    public override LocalizedString Tooltip => new LocalizedString(toolTipKey, Shortcut);
    public override string ToolNameLocalizationKey => toolName;
    public override string ToolName => toolName ?? base.ToolName;
    public bool IsCustomBrushTool { get; private set; }
    public KeyCombination? DefaultShortcut { get; set; }

    [Settings.Inherited] public double ToolSize => GetValue<double>();

    private string? toolName;
    private string toolTipKey;

    public BrushBasedToolViewModel()
    {
        Cursor = Cursors.PreciseCursor;
        Toolbar = CreateToolbar();

        (Toolbar as Toolbar).SettingChanged += OnSettingChanged;
    }

    public BrushBasedToolViewModel(Brush brush, string? tooltip, string? toolName, KeyCombination? defaultShortcut)
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

    private void OnSettingChanged(string name, object value)
    {
        if (name == nameof(BrushToolbar.Brush))
        {
            AddBrushShapeSettings();
        }
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
            if (blackboardVariable is VariableViewModel varVm)
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
}
