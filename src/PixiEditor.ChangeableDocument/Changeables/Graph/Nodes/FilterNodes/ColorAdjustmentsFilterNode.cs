using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ColorAdjustmentsFilter")]
public class ColorAdjustmentsFilterNode : FilterNode
{
    public InputProperty<bool> AdjustBrightness { get; }
    public InputProperty<double> BrightnessValue { get; }

    public InputProperty<bool> AdjustContrast { get; }
    public InputProperty<double> ContrastValue { get; }

    public InputProperty<bool> AdjustTemperature { get; }
    public InputProperty<double> TemperatureValue { get; }

    public InputProperty<bool> AdjustTint { get; }
    public InputProperty<double> TintValue { get; }

    public InputProperty<bool> AdjustSaturation { get; }
    public InputProperty<double> SaturationValue { get; }

    public InputProperty<bool> AdjustHue { get; }
    public InputProperty<double> HueValue { get; }

    private List<ColorFilter> filters = new List<ColorFilter>();
    private List<ColorFilter> toDispose = new List<ColorFilter>();
    private ColorFilter lastCombinedFilter;

    public ColorAdjustmentsFilterNode()
    {
        AdjustBrightness = CreateInput("AdjustBrightness", "ADJUST_BRIGHTNESS", false);
        BrightnessValue = CreateInput("BrightnessValue", "BRIGHTNESS_VALUE", 0.0)
            .WithRules(rules => rules.Min(-1d).Max(1d));

        AdjustContrast = CreateInput("AdjustContrast", "ADJUST_CONTRAST", false);
        ContrastValue = CreateInput("ContrastValue", "CONTRAST_VALUE", 0.0)
            .WithRules(rules => rules.Min(-1d).Max(1d));

        AdjustTemperature = CreateInput("AdjustTemperature", "ADJUST_TEMPERATURE", false);
        TemperatureValue = CreateInput("TemperatureValue", "TEMPERATURE_VALUE", 0.0)
            .WithRules(rules => rules.Min(-1d).Max(1d));

        AdjustTint = CreateInput("AdjustTint", "ADJUST_TINT", false);
        TintValue = CreateInput("TintValue", "TINT_VALUE", 0.0)
            .WithRules(rules => rules.Min(-1d).Max(1d));

        AdjustSaturation = CreateInput("AdjustSaturation", "ADJUST_SATURATION", false);
        SaturationValue = CreateInput("SaturationValue", "SATURATION_VALUE", 0.0)
            .WithRules(rules => rules.Min(-1d).Max(1d));

        AdjustHue = CreateInput("AdjustHue", "ADJUST_HUE", false);
        HueValue = CreateInput("HueValue", "HUE_VALUE", 0.0)
            .WithRules(rules => rules.Min(-180d).Max(180d));
    }

    protected override ColorFilter? GetColorFilter(RenderContext context)
    {
        toDispose.AddRange(filters);
        filters.Clear();

        CreateBrightnessFilter();
        CreateContrastFilter();
        CreateTemperatureFilter();
        CreateTintFilter();
        CreateSaturationFilter();
        CreateHueFilter();

        lastCombinedFilter?.Dispose();
        lastCombinedFilter = CombineFilters();
        return lastCombinedFilter;
    }

    private void CreateBrightnessFilter()
    {
        if (AdjustBrightness.Value)
        {
            float brightnessValue = (float)BrightnessValue.Value;
            ColorFilter brightnessFilter = ColorFilter.CreateColorMatrix(
            [
                1, 0, 0, 0, brightnessValue,
                0, 1, 0, 0, brightnessValue,
                0, 0, 1, 0, brightnessValue,
                0, 0, 0, 1, 0
            ]);
            filters.Add(brightnessFilter);
        }
    }

    private void CreateContrastFilter()
    {
        if (AdjustContrast.Value)
        {
            float contrastValue = (float)ContrastValue.Value;
            ColorFilter contrastFilter =
                ColorFilter.CreateHighContrast(false, ContrastInvertMode.InvertBrightness, contrastValue);
            filters.Add(contrastFilter);
        }
    }

    private void CreateTemperatureFilter()
    {
        if (AdjustTemperature.Value)
        {
            float temperatureValue = (float)TemperatureValue.Value;
            ColorFilter temperatureFilter = ColorFilter.CreateColorMatrix(
            [
                1, 0, 0, 0, temperatureValue,
                0, 1, 0, 0, 0,
                0, 0, 1, 0, -temperatureValue,
                0, 0, 0, 1, 0
            ]);
            filters.Add(temperatureFilter);
        }
    }

    private void CreateTintFilter()
    {
        if (AdjustTint.Value)
        {
            float tintValue = (float)TintValue.Value;
            ColorFilter tintFilter = ColorFilter.CreateColorMatrix(
            [
                1, 0, 0, 0, 0,
                0, 1, 0, 0, tintValue,
                0, 0, 1, 0, 0,
                0, 0, 0, 1, 0
            ]);
            filters.Add(tintFilter);
        }
    }

    private void CreateSaturationFilter()
    {
        if (AdjustSaturation.Value)
        {
            float saturationValue = (float)SaturationValue.Value + 1;
            ColorFilter saturationFilter = ColorFilter.CreateColorMatrix(
            [
                0.213f + 0.787f * saturationValue, 0.715f - 0.715f * saturationValue, 0.072f - 0.072f * saturationValue,
                0, 0,
                0.213f - 0.213f * saturationValue, 0.715f + 0.285f * saturationValue, 0.072f - 0.072f * saturationValue,
                0, 0,
                0.213f - 0.213f * saturationValue, 0.715f - 0.715f * saturationValue, 0.072f + 0.928f * saturationValue,
                0, 0,
                0, 0, 0, 1, 0
            ]);
            filters.Add(saturationFilter);
        }
    }

    private void CreateHueFilter()
    {
        if (AdjustHue.Value)
        {
            float value = (float)-HueValue.Value * (float)Math.PI / 180f;
            var cosVal = (float)Math.Cos(value);
            var sinVal = (float)Math.Sin(value);
            float lumR = 0.213f;
            float lumG = 0.715f;
            float lumB = 0.072f;

            ColorFilter hueFilter = ColorFilter.CreateColorMatrix(
            [
                lumR + cosVal * (1 - lumR) + sinVal * (-lumR), lumG + cosVal * (-lumG) + sinVal * (-lumG),
                lumB + cosVal * (-lumB) + sinVal * (1 - lumB), 0, 0,
                lumR + cosVal * (-lumR) + sinVal * (0.143f), lumG + cosVal * (1 - lumG) + sinVal * (0.140f),
                lumB + cosVal * (-lumB) + sinVal * (-0.283f), 0, 0,
                lumR + cosVal * (-lumR) + sinVal * (-(1 - lumR)), lumG + cosVal * (-lumG) + sinVal * (lumG),
                lumB + cosVal * (1 - lumB) + sinVal * (lumB), 0, 0,
                0, 0, 0, 1, 0,
            ]);

            filters.Add(hueFilter);
        }
    }

    private ColorFilter? CombineFilters()
    {
        if (filters.Count == 0)
        {
            return null;
        }

        ColorFilter combinedFilter = filters[0];
        for (int i = 1; i < filters.Count; i++)
        {
            combinedFilter = ColorFilter.CreateCompose(combinedFilter, filters[i]);
        }

        return combinedFilter;
    }

    public override Node CreateCopy()
    {
        return new ColorAdjustmentsFilterNode();
    }

    public override void Dispose()
    {
        base.Dispose();
        foreach (var filter in toDispose)
        {
            filter.Dispose();
        }

        lastCombinedFilter?.Dispose();
    }
}
