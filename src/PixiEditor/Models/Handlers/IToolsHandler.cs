using System.Collections.Generic;
using Avalonia.Input;
using PixiEditor.Models.Preferences;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Config;
using PixiEditor.Models.Events;
using PixiEditor.Numerics;
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
    public event EventHandler<SelectedToolEventArgs> SelectedToolChanged;
    public void SetupTools(IServiceProvider services, ToolSetsConfig toolSetConfig);
    public void SetupToolsTooltipShortcuts(IServiceProvider services);
    public void SetActiveTool<T>(bool transient) where T : IToolHandler;
    public void SetActiveTool(Type toolType, bool transient);
    public void ConvertedKeyDownInlet(FilteredKeyEventArgs args);
    public void ConvertedKeyUpInlet(FilteredKeyEventArgs args);
    public void HandleToolRepeatShortcutDown();
    public void HandleToolShortcutUp();
    public void UseToolEventInlet(VecD argsPositionOnCanvas, MouseButton argsButton);
    public T GetTool<T>() where T : IToolHandler;
    public void AddPropertyChangedCallback(string propertyName, Action callback);
}
