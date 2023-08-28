﻿using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Handlers.Toolbars;
using PixiEditor.AvaloniaUI.Models.Input;
using PixiEditor.AvaloniaUI.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.AvaloniaUI.Views.Overlays.BrushShapeOverlay;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Tools;

internal abstract class ToolViewModel : ObservableObject, IToolHandler
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

    /// <summary>
    /// Determines if secondary color should be used if right click mode is set to secondary color
    /// </summary>
    public virtual bool UsesColor => false;

   /// <summary>
   /// Determines if PixiEditor should switch to the Eraser when right click mode is set to erase
   /// </summary>
    public virtual bool IsErasable => false;

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

    private void OnLanguageChanged(Language obj)
    {
        ActionDisplay = new LocalizedString(ActionDisplay.Key);
    }

    public virtual void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown) { }
    public virtual void UseTool(VecD pos) { }
    public virtual void OnSelected() 
    {
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
    }

    public virtual void OnDeselecting()
    { }

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