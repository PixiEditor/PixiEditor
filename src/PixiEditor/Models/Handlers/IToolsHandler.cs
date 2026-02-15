using System.Collections.Generic;
using Avalonia.Input;
using PixiEditor.Models.Preferences;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Config;
using PixiEditor.Models.Events;
using Drawie.Numerics;
using PixiEditor.ViewModels.Tools;

namespace PixiEditor.Models.Handlers;

internal interface IToolsHandler : IHandler
{
    public void SetTool(object parameter);
    public void RestorePreviousTool();
    public IToolHandler ActiveTool { get; }
    public IToolSetHandler ActiveToolSet { get; } 
    public ICollection<IToolSetHandler> AllToolSets { get; }
    public RightClickMode RightClickMode { get; set; }
    public bool EnableSharedToolbar { get; set; }
    public bool SelectionTintingEnabled { get; set; }
    public event EventHandler<SelectedToolEventArgs> SelectedToolChanged;
    public void SetupTools(IServiceProvider services, ToolsConfig toolsConfig);
    public void SetupToolsTooltipShortcuts();
    public void SetActiveTool<T>(bool transient) where T : IToolHandler;
    public void SetActiveTool(Type toolType, bool transient);
    public void ConvertedKeyDownInlet(FilteredKeyEventArgs args);
    public void ConvertedKeyUpInlet(FilteredKeyEventArgs args);
    public void HandleToolRepeatShortcutDown();
    public void HandleToolShortcutUp();
    public void UseToolEventInlet(VecD argsPositionOnCanvas, MouseButton argsButton);
    public T GetTool<T>() where T : IToolHandler;
    public void AddPropertyChangedCallback(string propertyName, Action callback);
    public void OnPostUndoInlet();
    public void OnPostRedoInlet();
    public void OnPreRedoInlet();
    public void OnPreUndoInlet();
    public void QuickToolSwitchInlet();
    public void ChangeToolSize(double by);
    public bool CreateLayerIfNeeded();
    public bool NeedsNewLayerForActiveTool();
    public bool NeedsNewAnimationKeyFrameForActiveTool();
    public void DeselectActiveTool();
    public void CreateAnimationKeyFrameIfNeeded();
}
