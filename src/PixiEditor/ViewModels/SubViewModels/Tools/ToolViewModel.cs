using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Localization;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools;

internal abstract class ToolViewModel : NotifyableObject
{
    public bool IsTransient { get; set; } = false;
    public KeyCombination Shortcut { get; set; }

    public virtual string ToolName => GetType().Name.Replace("Tool", string.Empty).Replace("ViewModel", string.Empty);

    public abstract string ToolNameLocalizationKey { get; }
    public virtual LocalizedString DisplayName => new LocalizedString(ToolNameLocalizationKey);

    public virtual string ImagePath => $"/Images/Tools/{ToolName}Image.png";

    public virtual BrushShape BrushShape => BrushShape.Square;

    public virtual bool HideHighlight { get; }

    public abstract LocalizedString Tooltip { get; }

    private LocalizedString actionDisplay = string.Empty;
    public LocalizedString ActionDisplay
    {
        get => actionDisplay;
        set
        {
            actionDisplay = value;
            RaisePropertyChanged(nameof(ActionDisplay));
        }
    }

    private bool isActive;
    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            RaisePropertyChanged(nameof(IsActive));
        }
    }

    public Cursor Cursor { get; set; } = Cursors.Arrow;

    public Toolbar Toolbar { get; set; } = new EmptyToolbar();

    public virtual void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown) { }
    public virtual void OnLeftMouseButtonDown(VecD pos) { }
    public virtual void OnSelected() 
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
    }

    protected T GetValue<T>([CallerMemberName] string name = null)
    {
        var setting = Toolbar.GetSetting(name);

        if (setting.GetSettingType().IsAssignableTo(typeof(Enum)))
        {
            var property = setting.GetType().GetProperty("Value",  BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            return (T)property!.GetValue(setting);
        }

        return (T)setting.Value;
    }
}
