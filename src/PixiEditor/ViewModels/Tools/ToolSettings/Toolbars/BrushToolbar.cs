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

    public float Stabilization
    {
        get => GetSetting<FloatSettingViewModel>(nameof(Stabilization)).Value;
        set => GetSetting<FloatSettingViewModel>(nameof(Stabilization)).Value = value;
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
    }

    public BrushToolbar()
    {
        AddSetting(new BoolSettingViewModel(nameof(AntiAliasing), "ANTI_ALIASING_SETTING") { IsExposed = false });
        var setting = new SizeSettingViewModel(nameof(ToolSize), "TOOL_SIZE_LABEL", decimalPlaces: 1, min: 0.1);
        setting.ValueChanged += (_, _) => OnPropertyChanged(nameof(ToolSize));
        AddSetting(setting);
        AddSetting(new BrushSettingViewModel(nameof(Brush), "BRUSH_SETTING") { IsExposed = true });
        AddSetting(new FloatSettingViewModel(nameof(Stabilization), 0, "STABILIZATION_SETTING", min: 0, max: 15) { IsExposed = true });

        foreach (var aSetting in Settings)
        {
            if (aSetting.Name == "Brush" || aSetting.Name == "AntiAliasing" || aSetting.Name == "ToolSize")
            {
                aSetting.ValueChanged += SettingOnValueChanged;
            }
        }
    }

    private void SettingOnValueChanged(object? sender, SettingValueChangedEventArgs<object> e)
    {
        LastBrushData = CreateBrushData();
        OnPropertyChanged(nameof(LastBrushData));
    }
}
