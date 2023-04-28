using System.Reflection;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

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

            if (attribute is Settings.InheritedAttribute)
            {
                ProcessInheritedSetting(toolType, tool, toolbar, property, attribute, name);
            }
            else
            {
                var setting = CreateSetting(property.PropertyType, name, attribute, label);
                AddValueChangedHandlerIfRequired(toolType, tool, setting, attribute);
                toolbar.Settings.Add(setting);
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
        string label)
    {
        return attribute switch
        {
            Settings.BoolAttribute => new BoolSetting(name, (bool)(attribute.DefaultValue ?? false), label),
            Settings.ColorAttribute => new ColorSetting(name,
                ((Color)(attribute.DefaultValue ?? Colors.White)).ToColor(), label),
            Settings.EnumAttribute => GetEnumSetting(propertyType, name, attribute),
            Settings.FloatAttribute => new FloatSetting(name, (float)(attribute.DefaultValue ?? 0f), label),
            Settings.SizeAttribute => new SizeSetting(name, label),
            _ => throw new NotImplementedException(
                $"SettingsAttribute of type '{attribute.GetType().FullName}' has not been implemented")
        };
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
        return (Setting)typeof(EnumSetting<>)
            .MakeGenericType(enumType)
            .GetConstructor(new[] { typeof(string), typeof(string), enumType })!
            .Invoke(new[] { name, attribute.LabelKey ?? name, attribute.DefaultValue });
    }
}
