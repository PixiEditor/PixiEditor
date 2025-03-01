using Avalonia;
using Avalonia.Media;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.Helpers;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Document.Nodes.FilterNodes;

[NodeViewModel("COLOR_ADJUSTMENTS_FILTER", "FILTERS", PixiPerfectIcons.Sun )]
internal class ColorAdjustmentsFilterNodeViewModel : NodeViewModel<ColorAdjustmentsFilterNode>
{
    private Dictionary<string, List<INodePropertyHandler>> toggleToProperties = new Dictionary<string, List<INodePropertyHandler>>();
    public override void OnInitialized()
    {
        foreach (var input in Inputs)
        {
            if(input is BooleanPropertyViewModel booleanProperty)
            {
                booleanProperty.ValueChanged += BooleanPropertyOnValueChanged;
                toggleToProperties.Add(input.PropertyName, new List<INodePropertyHandler>());
            }
            if (input is DoublePropertyViewModel doubleProperty)
            {
                doubleProperty.NumberPickerMode = NumberPickerMode.Slider;
                doubleProperty.Min = -1;
                doubleProperty.Max = 1;

                if(input.PropertyName == "HueValue")
                {
                    doubleProperty.Min = -180;
                    doubleProperty.Max = 180;
                }

                doubleProperty.IsVisible = false;
                doubleProperty.SliderSettings.IsColorSlider = true;
                var background = SolveBackground(doubleProperty.PropertyName);

                doubleProperty.SliderSettings.BackgroundBrush = background;
                AddToToggleGroup(input);
            }
        }
    }

    private static IBrush SolveBackground(string propertyName)
    {
        if (propertyName.Contains("Brightness") || propertyName.Contains("Saturation") || propertyName.Contains("Contrast"))
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.Black, 0));
            brush.GradientStops.Add(new GradientStop(Colors.White, 1));

            return brush;
        }

        if (propertyName.Contains("Temperature"))
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.Blue, 0));
            brush.GradientStops.Add(new GradientStop(Colors.Red, 1));

            return brush;
        }

        if (propertyName.Contains("Tint"))
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.Green, 0));
            brush.GradientStops.Add(new GradientStop(Colors.Magenta, 1));

            return brush;
        }

        if (propertyName.Contains("Hue"))
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.Red, 0));
            brush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.166));
            brush.GradientStops.Add(new GradientStop(Colors.Green, 0.333));
            brush.GradientStops.Add(new GradientStop(Colors.Cyan, 0.5));
            brush.GradientStops.Add(new GradientStop(Colors.Blue, 0.666));
            brush.GradientStops.Add(new GradientStop(Colors.Magenta, 0.833));
            brush.GradientStops.Add(new GradientStop(Colors.Red, 1));

            return brush;
        }

        return ThemeResources.ThemeControlLowBrush;
    }

    private void BooleanPropertyOnValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        if (toggleToProperties.TryGetValue(property.PropertyName, out var toProperty))
        {
            foreach (var prop in toProperty)
            {
                prop.IsVisible = args.NewValue is bool b and true;
            }
        }
    }

    private void AddToToggleGroup(INodePropertyHandler property)
    {
        string groupName = "Adjust" + property.PropertyName.Replace("Value", "");
        if (toggleToProperties.ContainsKey(groupName))
        {
            toggleToProperties[groupName].Add(property);
        }
    }
}
