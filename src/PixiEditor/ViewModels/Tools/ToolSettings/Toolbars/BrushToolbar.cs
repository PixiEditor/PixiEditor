using System.ComponentModel;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal class BrushToolbar : Toolbar, IBrushToolbar
{
    public bool AntiAliasing
    {
        get => GetSetting<BoolSettingViewModel>(nameof(AntiAliasing)).Value;
        set => GetSetting<BoolSettingViewModel>(nameof(AntiAliasing)).Value = value;
    }

    public double ToolSize
    {
        get => GetSetting<SizeSettingViewModel>(nameof(ToolSize)).Value;
        set => GetSetting<SizeSettingViewModel>(nameof(ToolSize)).Value = value;
    }

    public Brush Brush
    {
        get => GetSetting<BrushSettingViewModel>(nameof(Brush)).Value;
        set => GetSetting<BrushSettingViewModel>(nameof(Brush)).Value = value;
    }

    public double Stabilization
    {
        get => GetSetting<SizeSettingViewModel>(nameof(Stabilization)).Value;
        set => GetSetting<SizeSettingViewModel>(nameof(Stabilization)).Value = value;
    }

    public StabilizationMode StabilizationMode
    {
        get => GetSetting<EnumSettingViewModel<StabilizationMode>>(nameof(StabilizationMode)).Value;
        set => GetSetting<EnumSettingViewModel<StabilizationMode>>(nameof(StabilizationMode)).Value = value;
    }

    public BrushData CreateBrushData()
    {
        Brush? brush = Brush;
        if (brush == null)
        {
            return new BrushData();
        }

        var pipe = Brush.Document.ShareGraph();
        var data = new BrushData(pipe.TryAccessData()) { AntiAliasing = AntiAliasing, StrokeWidth = (float)ToolSize };

        pipe.Dispose();
        return data;
    }

    public BrushData LastBrushData { get; private set; } = new BrushData();

    public override void OnLoadedSettings()
    {
        OnPropertyChanged(nameof(ToolSize));
        OnPropertyChanged(nameof(Brush));
        OnPropertyChanged(nameof(AntiAliasing));
        OnPropertyChanged(nameof(Stabilization));
        OnPropertyChanged(nameof(StabilizationMode));
    }

    public BrushToolbar()
    {
        AddSetting(new BoolSettingViewModel(nameof(AntiAliasing), "ANTI_ALIASING_SETTING") { IsExposed = false });
        var setting = new SizeSettingViewModel(nameof(ToolSize), "TOOL_SIZE_LABEL", decimalPlaces: 1, min: 0.1);
        setting.ValueChanged += (_, _) => OnPropertyChanged(nameof(ToolSize));
        AddSetting(setting);
        AddSetting(new BrushSettingViewModel(nameof(Brush), "BRUSH_SETTING") { IsExposed = true });
        AddSetting(new EnumSettingViewModel<StabilizationMode>(nameof(StabilizationMode), "STABILIZATION_MODE_SETTING") { IsExposed = true });
        AddSetting(new SizeSettingViewModel(nameof(Stabilization), "STABILIZATION_SETTING", 0, min: 0, max: 128) { IsExposed = true });

        foreach (var aSetting in Settings)
        {
            if (aSetting.Name is "Brush" or "AntiAliasing" or "ToolSize")
            {
                aSetting.ValueChanged += SettingOnValueChanged;
            }

            if(aSetting.Name == "Stabilization")
            {
                aSetting.ValueChanged += (_, _) => OnPropertyChanged(nameof(Stabilization));
            }

            if (aSetting.Name == "StabilizationMode")
            {
                aSetting.ValueChanged += (_, _) => OnPropertyChanged(nameof(StabilizationMode));
            }
        }
    }

    private void SettingOnValueChanged(object? sender, SettingValueChangedEventArgs<object> e)
    {
        LastBrushData = CreateBrushData();
        OnPropertyChanged(nameof(LastBrushData));
    }
}
