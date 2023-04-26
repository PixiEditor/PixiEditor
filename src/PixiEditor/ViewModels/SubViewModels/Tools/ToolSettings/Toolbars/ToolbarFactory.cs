using System.Reflection;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

internal static class ToolbarFactory
{
    public static Toolbar Create<T>() where T : ToolViewModel => Create<T, EmptyToolbar>();

    public static TToolbar Create<T, TToolbar>() where T : ToolViewModel where TToolbar : Toolbar, new()
    {
        var tool = typeof(T);
        var toolbar = new TToolbar();

        foreach (var property in tool.GetProperties())
        {
            var attribute = property.GetCustomAttribute<Settings.SettingsAttribute>();

            if (attribute == null)
                continue;
            
            var name = attribute.Name ?? property.Name;

            if (attribute is Settings.InheritedAttribute)
            {
                var inherited = toolbar.GetSetting(name);
                
                if (inherited == null)
                {
                    throw new NullReferenceException($"There's no inherited setting '{name}' on inherited toolbar of type '{typeof(TToolbar).FullName}' (Tool: {typeof(T).FullName})");
                }

                if (inherited.GetSettingType() != property.PropertyType)
                {
                    throw new InvalidCastException($"Inherited setting '{name}' does not match property type '{property.PropertyType}' (Tool: {typeof(T).FullName})");
                }

                continue;
            }
            
            var label = attribute.LabelKey ?? name;

            var setting = attribute switch
            {
                Settings.BoolAttribute => new BoolSetting(name, (bool)(attribute.DefaultValue ?? false), label),
                Settings.ColorAttribute => new ColorSetting(name, ((Color)(attribute.DefaultValue ?? Colors.White)).ToColor(), label),
                Settings.EnumAttribute => GetEnumSetting(property.PropertyType, name, attribute),
                Settings.FloatAttribute => new FloatSetting(name, (float)(attribute.DefaultValue ?? 0f), attribute.LabelKey),
                Settings.SizeAttribute => new SizeSetting(name, label),
                _ => throw new NotImplementedException($"SettingsAttribute of type '{attribute.GetType().FullName}' has not been implemented")
            };
            
            if (setting.GetSettingType() != property.PropertyType)
            {
                throw new InvalidCastException($"Setting '{name}' does not match property type '{property.PropertyType}' (Tool: {typeof(T).FullName})");
            }

            toolbar.Settings.Add(setting);
        }

        return toolbar;
    }

    private static Setting GetEnumSetting(Type enumType, string name, Settings.SettingsAttribute attribute)
    {
        return (Setting)typeof(EnumSetting<>)
            .MakeGenericType(enumType)
            .GetConstructor(new[] { typeof(string), typeof(string), enumType })!
            .Invoke(new[] { name, attribute.LabelKey ?? name, attribute.DefaultValue });
    }
}
