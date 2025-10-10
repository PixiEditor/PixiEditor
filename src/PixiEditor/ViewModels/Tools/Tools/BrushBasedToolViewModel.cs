using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Input;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document.Blackboard;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Tools.Tools;

internal abstract class BrushBasedToolViewModel : ToolViewModel, IBrushToolHandler
{
    private List<Setting> brushShapeSettings = new();
    public override Type[]? SupportedLayerTypes { get; } = { typeof(IRasterLayerHandler) };
    public override Type LayerTypeToCreateOnEmptyUse { get; } = typeof(ImageLayerNode);

    [Settings.Inherited] public double ToolSize => GetValue<double>();

    public BrushBasedToolViewModel()
    {
        Cursor = Cursors.PreciseCursor;
        Toolbar = CreateToolbar();

        (Toolbar as Toolbar).SettingChanged += OnSettingChanged;
    }

    protected abstract Toolbar CreateToolbar();

    protected abstract void SwitchToTool();

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
