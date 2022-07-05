using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Events;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Tools", "Tools")]
public class ToolsViewModel : SubViewModel<ViewModelMain>
{
    private Cursor toolCursor;
    private Tool activeTool;

    public Tool LastActionTool { get; private set; }

    public bool ActiveToolIsTransient { get; set; }

    public Cursor ToolCursor
    {
        get => toolCursor;
        set => SetProperty(ref toolCursor, value);
    }

    public Tool ActiveTool
    {
        get => activeTool;
        private set => SetProperty(ref activeTool, value);
    }

    public int ToolSize
    {
        get => ActiveTool.Toolbar.GetSetting<SizeSetting>("ToolSize") != null
            ? ActiveTool.Toolbar.GetSetting<SizeSetting>("ToolSize").Value
            : 1;
        set
        {
            if (ActiveTool.Toolbar.GetSetting<SizeSetting>("ToolSize") is SizeSetting toolSize)
            {
                toolSize.Value = value;
                Owner.BitmapManager.UpdateHighlightIfNecessary();
            }
        }
    }

    public List<Tool> ToolSet { get; private set; }

    public event EventHandler<SelectedToolEventArgs> SelectedToolChanged;

    public ToolsViewModel(ViewModelMain owner)
        : base(owner)
    { }

    public void SetupTools(IServiceProvider services)
    {
        ToolSet = services.GetServices<Tool>().ToList();
        SetActiveTool<PenTool>();
    }

    public void SetupToolsTooltipShortcuts(IServiceProvider services)
    {
        foreach (var tool in ToolSet)
        {
            tool.Shortcut = Owner.ShortcutController.GetToolShortcut(tool.GetType());
        }
    }

    public void SetActiveTool<T>()
        where T : Tool
    {
        SetActiveTool(typeof(T));
    }

    [Command.Internal("PixiEditor.Tools.SelectTool", CanExecute = "PixiEditor.HasDocument")]
    public void SetActiveTool(Tool tool)
    {
        if (ActiveTool == tool) return;

        if (!tool.Toolbar.SettingsGenerated)
        {
            tool.Toolbar.GenerateSettings();
        }

        ActiveToolIsTransient = false;
        bool shareToolbar = IPreferences.Current.GetPreference<bool>("EnableSharedToolbar");
        if (ActiveTool != null)
        {
            activeTool.IsActive = false;
            if (shareToolbar)
            {
                ActiveTool.Toolbar.SaveToolbarSettings();
            }
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
        Owner.BitmapManager.UpdateActionDisplay(LastActionTool);
        //update new tool
        Owner.BitmapManager.UpdateActionDisplay(ActiveTool);
        Owner.BitmapManager.UpdateHighlightIfNecessary();

        tool.IsActive = true;
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
            SetActiveTool(type);
            return;
        }

        Tool tool = (Tool)parameter;
        SetActiveTool(tool.GetType());
    }

    [Command.Basic("PixiEditor.Tools.IncreaseSize", 1, "Increase Tool Size", "Increase Tool Size", Key = Key.OemCloseBrackets)]
    [Command.Basic("PixiEditor.Tools.DecreaseSize", -1, "Decrease Tool Size", "Decrease Tool Size", Key = Key.OemOpenBrackets)]
    public void ChangeToolSize(int increment)
    {
        int newSize = ToolSize + increment;
        if (newSize > 0)
        {
            ToolSize = newSize;
        }
    }

    public void SetActiveTool(Type toolType)
    {
        if (!typeof(Tool).IsAssignableFrom(toolType)) { throw new ArgumentException($"'{toolType}' does not inherit from {typeof(Tool)}"); }
        Tool foundTool = ToolSet.First(x => x.GetType() == toolType);
        SetActiveTool(foundTool);
    }

    private void SetToolCursor(Type tool)
    {
        if (tool != null)
        {
            ToolCursor = ActiveTool.Cursor;
        }
        else
        {
            ToolCursor = Cursors.Arrow;
        }
    }
}
