using System.Reflection;
using Avalonia.Media;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal static class ToolbarFactory
{
    public static Toolbar Create<T>(T tool) where T : ToolViewModel => Create<T, EmptyToolbar>(tool);

    public static TToolbar Create<T, TToolbar>(T tool) where T : ToolViewModel where TToolbar : Toolbar, new()
    {
        var toolType = typeof(T);
        var toolbar = new TToolbar();

        foreach (var property in toolType.GetProperties())
        {
            var attribute = property.GetCustomAttribute<Settings.SettingsAttribute>();
            if (attribute == null) continue;

            var name = attribute.Name ?? property.Name;
            var label = attribute.LabelKey ?? name;
            bool exposedByDefault = attribute.ExposedByDefault;

            if (attribute is Settings.InheritedAttribute)
            {
                ProcessInheritedSetting(toolType, tool, toolbar, property, attribute, name);
            }
            else
            {
                var setting = CreateSetting(property.PropertyType, name, attribute, label, exposedByDefault);
                AddValueChangedHandlerIfRequired(toolType, tool, setting, attribute);
                toolbar.AddSetting(setting);
            }
        }

        return toolbar;
    }

    private static void ProcessInheritedSetting(Type toolType, ToolViewModel tool, Toolbar toolbar,
        PropertyInfo property, Settings.SettingsAttribute attribute, string name)
    {
        var inherited = toolbar.GetSetting(name);
        if (inherited == null || inherited.GetSettingType() != property.PropertyType)
        {
            throw new InvalidOperationException(
                $"Inherited setting '{name}' does not match property type '{property.PropertyType}' (Tool: {toolType.FullName})");
        }

        AddValueChangedHandlerIfRequired(toolType, tool, inherited, attribute);
    }

    private static Setting CreateSetting(Type propertyType, string name, Settings.SettingsAttribute attribute,
        string label, bool exposedByDefault)
    {
        var attr = attribute switch
        {
            Settings.BoolAttribute => new BoolSettingViewModel(name, (bool)(attribute.DefaultValue ?? false), label),
            Settings.ColorAttribute => new ColorSettingViewModel(name,
                ((IBrush)(attribute.DefaultValue ?? Brushes.White)), label),
            Settings.EnumAttribute => GetEnumSetting(propertyType, name, attribute),
            Settings.PercentAttribute percentAttribute => new PercentSettingViewModel(name, (float)(attribute.DefaultValue ?? 0f), label,
                percentAttribute.Min, percentAttribute.Max),
            Settings.FloatAttribute floatAttribute => new FloatSettingViewModel(name, (float)(attribute.DefaultValue ?? 0f), label,
                floatAttribute.Min, floatAttribute.Max),
            Settings.SizeAttribute => new SizeSettingViewModel(name, label),
            _ => throw new NotImplementedException(
                $"SettingsAttribute of type '{attribute.GetType().FullName}' has not been implemented")
        };
        
        attr.IsExposed = exposedByDefault;
        
        return attr;
    }

    private static void AddValueChangedHandlerIfRequired(Type toolType, ToolViewModel tool, Setting setting,
        Settings.SettingsAttribute attribute)
    {
        if (attribute.Notify != null)
        {
            AddValueChangedHandler(toolType, tool, setting, attribute);
        }
    }

    private static void AddValueChangedHandler<T>(Type toolType, T tool, Setting setting,
        Settings.SettingsAttribute attribute) where T : ToolViewModel
    {
        if (attribute.Notify != null)
        {
            var method = toolType.GetMethod(attribute.Notify,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                Array.Empty<Type>());

            if (method is null)
            {
                throw new NullReferenceException(
                    $"No method found with the name '{attribute.Notify}' that does not have any parameters");
            }

            setting.ValueChanged += (_, _) => method.Invoke(tool, null);
        }
    }

    private static Setting GetEnumSetting(Type enumType, string name, Settings.SettingsAttribute attribute)
    {
        return (Setting)typeof(EnumSettingViewModel<>)
            .MakeGenericType(enumType)
            .GetConstructor(new[] { typeof(string), typeof(string), enumType })!
            .Invoke(new[] { name, attribute.LabelKey ?? name, attribute.DefaultValue });
    }
}
