using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.SubViewModels.Main;
#nullable enable
[Command.Group("PixiEditor.Tools", "TOOLS")]
internal class ToolsViewModel : SubViewModel<ViewModelMain>
{
    private RightClickMode rightClickMode;
    
    public ZoomToolViewModel? ZoomTool => GetTool<ZoomToolViewModel>();

    public ToolViewModel? LastActionTool { get; private set; }

    public RightClickMode RightClickMode
    {
        get => rightClickMode;
        set
        {
            if (SetProperty(ref rightClickMode, value))
            {
                IPreferences.Current.UpdatePreference(nameof(RightClickMode), value);
            }
        }
    }

    public bool EnableSharedToolbar
    {
        get => IPreferences.Current.GetPreference<bool>(nameof(EnableSharedToolbar));
        set
        {
            if (EnableSharedToolbar == value)
            {
                return;
            }

            IPreferences.Current.UpdatePreference(nameof(EnableSharedToolbar), value);
            RaisePropertyChanged(nameof(EnableSharedToolbar));
        }
    }

    private Cursor? toolCursor;
    public Cursor? ToolCursor
    {
        get => toolCursor;
        set => SetProperty(ref toolCursor, value);
    }

    public BasicToolbar? ActiveBasicToolbar
    {
        get => ActiveTool?.Toolbar as BasicToolbar;
    }

    private ToolViewModel? activeTool;
    public ToolViewModel? ActiveTool
    {
        get => activeTool;
        private set
        {
            SetProperty(ref activeTool, value);
            RaisePropertyChanged(nameof(ActiveBasicToolbar));
        }
    }

    public List<ToolViewModel>? ToolSet { get; private set; }

    public event EventHandler<SelectedToolEventArgs>? SelectedToolChanged;

    private bool shiftIsDown;
    private bool ctrlIsDown;
    private bool altIsDown;

    private ToolViewModel _preTransientTool;


    public ToolsViewModel(ViewModelMain owner)
        : base(owner)
    {
        rightClickMode = IPreferences.Current.GetPreference<RightClickMode>(nameof(RightClickMode));
    }

    public void SetupTools(IServiceProvider services)
    {
        ToolSet = services.GetServices<ToolViewModel>().ToList();
    }

    public void SetupToolsTooltipShortcuts(IServiceProvider services)
    {
        foreach (ToolViewModel tool in ToolSet!)
        {
            tool.Shortcut = Owner.ShortcutController.GetToolShortcut(tool.GetType());
        }
    }

    public T? GetTool<T>()
        where T : ToolViewModel
    {
        return (T?)ToolSet?.Where(static tool => tool is T).FirstOrDefault();
    }

    public void SetActiveTool<T>(bool transient)
        where T : ToolViewModel
    {
        SetActiveTool(typeof(T), transient);
    }

    [Command.Basic("PixiEditor.Tools.ApplyTransform", "APPLY_TRANSFORM", "", Key = Key.Enter)]
    public void ApplyTransform()
    {
        DocumentViewModel? doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        doc.EventInlet.OnApplyTransform();
    }

    [Command.Internal("PixiEditor.Tools.SelectTool", CanExecute = "PixiEditor.HasDocument")]
    public void SetActiveTool(ToolViewModel tool)
    {
        SetActiveTool(tool, false);
    }

    public void SetActiveTool(ToolViewModel tool, bool transient)
    {
        if (ActiveTool == tool)
        {
            ActiveTool.IsTransient = transient;
            return;
        }

        ActiveTool?.OnDeselecting();

        if (!tool.Toolbar.SettingsGenerated)
            tool.Toolbar.GenerateSettings();

        if (ActiveTool != null) ActiveTool.IsTransient = false;
        bool shareToolbar = EnableSharedToolbar;
        if (ActiveTool is not null)
        {
            ActiveTool.IsActive = false;
            if (shareToolbar)
                ActiveTool.Toolbar.SaveToolbarSettings();
        }

        LastActionTool = ActiveTool;
        ActiveTool = tool;

        if (shareToolbar)
        {
            ActiveTool.Toolbar.LoadSharedSettings();
        }

        if (LastActionTool != ActiveTool)
            SelectedToolChanged?.Invoke(this, new SelectedToolEventArgs(LastActionTool, ActiveTool));

        //update old tool
        LastActionTool?.ModifierKeyChanged(false, false, false);
        //update new tool
        ActiveTool.ModifierKeyChanged(ctrlIsDown, shiftIsDown, altIsDown);
        ActiveTool.OnSelected();

        tool.IsActive = true;
        ActiveTool.IsTransient = transient;
        SetToolCursor(tool.GetType());

        if (Owner.StylusSubViewModel != null)
        {
            Owner.StylusSubViewModel.ToolSetByStylus = false;
        }
    }

    public void SetTool(object parameter)
    {
        if (parameter is Type type)
        {
            SetActiveTool(type, false);
            return;
        }

        ToolViewModel tool = (ToolViewModel)parameter;
        SetActiveTool(tool.GetType(), false);
    }

    [Command.Basic("PixiEditor.Tools.IncreaseSize", 1, "INCREASE_TOOL_SIZE", "INCREASE_TOOL_SIZE", CanExecute = "PixiEditor.Tools.CanChangeToolSize", Key = Key.OemCloseBrackets)]
    [Command.Basic("PixiEditor.Tools.DecreaseSize", -1, "DECREASE_TOOL_SIZE", "DECREASE_TOOL_SIZE", CanExecute = "PixiEditor.Tools.CanChangeToolSize", Key = Key.OemOpenBrackets)]
    public void ChangeToolSize(int increment)
    {
        if (ActiveTool?.Toolbar is not BasicToolbar toolbar)
            return;
        int newSize = toolbar.ToolSize + increment;
        if (newSize > 0)
            toolbar.ToolSize = newSize;
    }

    [Evaluator.CanExecute("PixiEditor.Tools.CanChangeToolSize")]
    public bool CanChangeToolSize() => Owner.ToolsSubViewModel.ActiveTool.Toolbar is BasicToolbar
                                       && Owner.ToolsSubViewModel.ActiveTool is not PenToolViewModel
                                       {
                                           PixelPerfectEnabled: true
                                       };

    public void SetActiveTool(Type toolType, bool transient)
    {
        if (!typeof(ToolViewModel).IsAssignableFrom(toolType))
            throw new ArgumentException($"'{toolType}' does not inherit from {typeof(ToolViewModel)}");
        ToolViewModel foundTool = ToolSet!.First(x => x.GetType() == toolType);
        SetActiveTool(foundTool, transient);
    }
    
    public void RestorePreviousTool()
    {
        if (LastActionTool != null)
        {
            SetActiveTool(LastActionTool, false);
        }
        else
        {
            SetActiveTool<PenToolViewModel>(false);
        }
    }

    private void SetToolCursor(Type tool)
    {
        if (tool is not null)
        {
            ToolCursor = ActiveTool?.Cursor;
        }
        else
        {
            ToolCursor = Cursors.Arrow;
        }
    }

    public void HandleToolRepeatShortcutDown()
    {
        if(ActiveTool == null) return;
        if (ActiveTool is null or { IsTransient: false })
        {
            ShortcutController.BlockShortcutExecution("ShortcutDown");
            ActiveTool.IsTransient = true;
        }
    }
    
    public void HandleToolShortcutUp()
    {
        if(ActiveTool == null) return;
        if (ActiveTool.IsTransient && LastActionTool is { } tool)
            SetActiveTool(tool, false);
        ShortcutController.UnblockShortcutExecution("ShortcutDown");
    }

    public void UseToolEventInlet(VecD canvasPos, MouseButton button)
    {
        if (ActiveTool == null) return;

        ActiveTool.UsedWith = button;
        ActiveTool.UseTool(canvasPos);
    }

    public void ConvertedKeyDownInlet(FilteredKeyEventArgs args)
    {
        ActiveTool?.ModifierKeyChanged(args.IsCtrlDown, args.IsShiftDown, args.IsAltDown);
    }

    public void ConvertedKeyUpInlet(FilteredKeyEventArgs args)
    {
        ActiveTool?.ModifierKeyChanged(args.IsCtrlDown, args.IsShiftDown, args.IsAltDown);
    }
}
