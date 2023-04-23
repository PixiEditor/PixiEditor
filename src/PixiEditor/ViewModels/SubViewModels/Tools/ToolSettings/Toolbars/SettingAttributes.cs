using System.Reflection.Emit;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

public static class Settings
{
    /// <summary>
    /// A toolbar setting of type <see cref="bool"/>
    /// </summary>
    public class BoolAttribute : SettingsAttribute
    {
        public BoolAttribute(string label) : base(label) { }

        public BoolAttribute(string label, object defaultValue) : base(label, defaultValue) { }
    }

    /// <summary>
    /// A toolbar setting of any enum
    /// </summary>
    public class EnumAttribute : SettingsAttribute
    {
        public EnumAttribute(string label) : base(label) { }

        public EnumAttribute(string label, object defaultValue) : base(label, defaultValue) { }
    }

    /// <summary>
    /// A toolbar setting of type <see cref="Color"/>
    /// </summary>
    public class ColorAttribute : SettingsAttribute
    {
        public ColorAttribute(string label) : base(label) { }

        public ColorAttribute(string label, byte r, byte g, byte b) : base(label, new Color(r, g, b)) { }
        
        public ColorAttribute(string label, byte r, byte g, byte b, byte a) : base(label, new Color(r, g, b, a)) { }
    }

    /// <summary>
    /// A toolbar setting of type <see cref="float"/>
    /// </summary>
    public class FloatAttribute : SettingsAttribute
    {
        public FloatAttribute(string label) : base(label) { }

        public FloatAttribute(string label, float defaultValue) : base(label, defaultValue) { }
    }

    /// <summary>
    /// A toolbar setting of type <see cref="int"/>
    /// </summary>
    public class SizeAttribute : SettingsAttribute
    {
        public SizeAttribute(string label) : base(label) { }
    }

    /// <summary>
    /// Marks a setting to be inherited from the toolbar type
    /// </summary>
    public class InheritedAttribute : SettingsAttribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class SettingsAttribute : Attribute
    {
        public string Name { get; set; }
        
        public string Notify { get; set; }

        public SettingsAttribute() { }
        
        public SettingsAttribute(string label)
        {
            Label = label;
        }

        public SettingsAttribute(string label, object defaultValue)
        {
            Label = label;
            DefaultValue = defaultValue;
        }
        
        public readonly string Label;

        public readonly object DefaultValue;
    }
}
